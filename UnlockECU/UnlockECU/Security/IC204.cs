using System;
using System.Collections.Generic;

namespace UnlockECU
{
    /// <summary>
    /// IC204 Seed-Key Algorithm for Mercedes W204 instrument cluster (NEC V850E1).
    /// Supports 31 SW versions across 6 firmware series (197902, 204442, 204902, 212442, 212902, 218902).
    ///
    /// Algorithm overview:
    ///   1. Permute 8-byte seed into workspace
    ///   2. Compute LFSR iteration count from a permuted seed byte selected by the internal level
    ///   3. Generate 8-byte LFSR state from the salt via salt_transform (level-dependent)
    ///   4. Run LFSR: feedback XOR of specific bit taps, then rotate-right the 8-byte state
    ///   5. Feistel-like network (2 rounds, each with 2 half-rounds):
    ///      - XOR, rotate, 32-bit LE add with LFSR state, swap halves
    ///   6. Reverse-permute the result to produce the 8-byte key
    ///
    /// Parameters (from db.json):
    ///   "Salt" (ByteArray, 8 bytes): per-level salt table entry
    ///
    /// The UDS access level is mapped to an internal level (1-7) used by the algorithm.
    /// </summary>
    class IC204 : SecurityProvider
    {
        // UDS SecurityAccess sub-function to internal level mapping.
        // UDS 0x01 -> internal 1, UDS 0x03 -> internal 3, UDS 0x09 -> internal 5, UDS 0x0D -> internal 7
        private static int UdsToInternalLevel(int udsLevel)
        {
            return udsLevel switch
            {
                0x01 => 1,
                0x03 => 3,
                0x09 => 5,
                0x0D => 7,
                _ => (udsLevel + 1) / 2,
            };
        }

        /// <summary>
        /// Right-rotate a byte array (treated as a contiguous bitfield) by 1 bit.
        /// The LSB of byte[i-1] becomes the MSB of byte[i], wrapping around.
        /// </summary>
        private static void RotateRightArray(byte[] buf, int offset, int length)
        {
            if (length == 0 || length > 8)
                return;

            // Collect carry bits before modifying
            int[] carries = new int[length];
            for (int i = 0; i < length; i++)
                carries[i] = buf[offset + i] & 1;

            for (int i = 0; i < length; i++)
            {
                int carryIdx = (i == 0) ? length - 1 : i - 1;
                byte shifted = (byte)((buf[offset + i] >> 1) & 0x7F);
                if (carries[carryIdx] != 0)
                    shifted |= 0x80;
                buf[offset + i] = shifted;
            }
        }

        /// <summary>
        /// FUN_00180bd2: Generate 8-byte LFSR initial state from salt only.
        /// </summary>
        private static void SaltTransform(byte[] salt, byte[] output)
        {
            byte[] local = new byte[0x14];

            // Store salt at local[0x0C..0x13]
            Array.Copy(salt, 0, local, 0x0C, 8);

            // Phase 1: salt[4..7] -> local[0x00..0x03]
            for (int i = 0; i < 4; i++)
                local[i] = local[0x10 + i];

            // Phase 2: salt[0..3] -> local[0x04..0x07]
            for (int i = 0; i < 4; i++)
                local[0x04 + i] = local[0x0C + i];

            // Phase 3: local[0x00..0x03] -> local[0x08..0x0B]
            for (int i = 0; i < 4; i++)
                local[0x08 + i] = local[i];

            // Phase 4: Transpose local[0x08..0x0B] -> local[0x00..0x03]
            byte t0 = local[0x08], t1 = local[0x09], t2 = local[0x0A], t3 = local[0x0B];
            local[0x02] = t0;
            local[0x00] = t1;
            local[0x01] = t3;
            local[0x03] = t2;

            // Phase 5: Rotate local[0x00..0x03] right by (t2 & 0xF) + 1
            int rotateCount = (t2 & 0x0F) + 1;
            for (int r = 0; r < rotateCount; r++)
                RotateRightArray(local, 0, 4);

            // Phase 6: Rotate local[0x08..0x0B] right by (local[0x0B] & 0xF) + 1
            rotateCount = (local[0x0B] & 0x0F) + 1;
            for (int r = 0; r < rotateCount; r++)
                RotateRightArray(local, 0x08, 4);

            // Phase 7: XOR local[0..3] ^= local[0x08..0x0B]
            for (int i = 0; i < 4; i++)
                local[i] ^= local[0x08 + i];

            // Phase 8: local[0x04..0x07] -> local[0x08..0x0B]
            for (int i = 0; i < 4; i++)
                local[0x08 + i] = local[0x04 + i];

            // Phase 9: Transpose local[0x08..0x0B] -> local[0x04..0x07]
            t0 = local[0x08]; t1 = local[0x09]; t2 = local[0x0A]; t3 = local[0x0B];
            local[0x06] = t0;
            local[0x04] = t1;
            local[0x05] = t3;
            local[0x07] = t2;

            // Phase 10: Rotate local[0x04..0x07] right by (local[0x0A] & 0xF) + 1
            rotateCount = (local[0x0A] & 0x0F) + 1;
            for (int r = 0; r < rotateCount; r++)
                RotateRightArray(local, 0x04, 4);

            // Phase 11: Rotate local[0x08..0x0B] right by (local[0x0B] & 0xF) + 1
            rotateCount = (local[0x0B] & 0x0F) + 1;
            for (int r = 0; r < rotateCount; r++)
                RotateRightArray(local, 0x08, 4);

            // Phase 12: XOR local[0x04..0x07] ^= local[0x08..0x0B]
            for (int i = 0; i < 4; i++)
                local[0x04 + i] ^= local[0x08 + i];

            // Phase 13: local[0x00..0x03] -> local[0x10..0x13]
            for (int i = 0; i < 4; i++)
                local[0x10 + i] = local[i];

            // Phase 14: local[0x04..0x07] -> local[0x0C..0x0F]
            for (int i = 0; i < 4; i++)
                local[0x0C + i] = local[0x04 + i];

            // Output: local[0x0C..0x13] (8 bytes)
            Array.Copy(local, 0x0C, output, 0, 8);
        }

        /// <summary>
        /// LFSR feedback: XOR 8 specific bit taps from ws[0x08..0x0F],
        /// write result into bit 7 of ws[0x0F].
        /// </summary>
        private static void ComputeLfsrFeedback(byte[] ws)
        {
            int fb = (ws[0x08] >> 3) & 1;
            fb ^= ws[0x09] & 1;
            fb ^= (ws[0x0A] >> 1) & 1;
            fb ^= (ws[0x0B] >> 7) & 1;
            fb ^= (ws[0x0C] >> 5) & 1;
            fb ^= (ws[0x0D] >> 2) & 1;
            fb ^= (ws[0x0E] >> 6) & 1;
            fb ^= (ws[0x0F] >> 4) & 1;
            ws[0x0F] = (byte)((ws[0x0F] & 0x7F) | (fb << 7));
        }

        private static uint Le32Load(byte[] buf, int offset)
        {
            return (uint)(buf[offset + 3] << 24)
                 | (uint)(buf[offset + 2] << 16)
                 | (uint)(buf[offset + 1] << 8)
                 | (uint)buf[offset];
        }

        private static void Le32Store(byte[] buf, int offset, uint val)
        {
            buf[offset] = (byte)(val & 0xFF);
            buf[offset + 1] = (byte)((val >> 8) & 0xFF);
            buf[offset + 2] = (byte)((val >> 16) & 0xFF);
            buf[offset + 3] = (byte)((val >> 24) & 0xFF);
        }

        public override bool GenerateKey(byte[] inSeed, byte[] outKey, int accessLevel, List<Parameter> parameters)
        {
            byte[] salt = GetParameterBytearray(parameters, "Salt");

            if (inSeed.Length != 8 || outKey.Length != 8 || salt.Length != 8)
                return false;

            int level = UdsToInternalLevel(accessLevel);
            if (level < 1 || level > 7)
                return false;

            // Workspace: ws[0x00..0x07] = working copy, ws[0x08..0x0F] = LFSR state, ws[0x10..0x17] = permuted seed
            byte[] ws = new byte[0x20];

            // Step 1: Permute seed bytes into ws[0x10..0x17]
            ws[0x10] = inSeed[7];
            ws[0x11] = inSeed[4];
            ws[0x12] = inSeed[3];
            ws[0x13] = inSeed[6];
            ws[0x14] = inSeed[5];
            ws[0x15] = inSeed[1];
            ws[0x16] = inSeed[0];
            ws[0x17] = inSeed[2];

            // Step 2: LFSR iteration count from permuted seed byte at (0x10 + level)
            int lfsrCount = (ws[0x10 + level] & 0x07) + 2;

            // Step 3: Generate LFSR initial state from salt
            byte[] lfsrState = new byte[8];
            SaltTransform(salt, lfsrState);
            Array.Copy(lfsrState, 0, ws, 0x08, 8);

            // Step 4: Run LFSR (feedback + rotate) for lfsrCount iterations
            for (int i = 0; i < lfsrCount; i++)
            {
                ComputeLfsrFeedback(ws);
                RotateRightArray(ws, 0x08, 8);
            }

            // Step 5: Copy permuted seed to working area
            Array.Copy(ws, 0x10, ws, 0x00, 8);

            // Step 6: Feistel-like network - 2 rounds, each with 2 half-rounds
            for (int round = 0; round < 2; round++)
            {
                // Half-round A: uses ws[0x08..0x0B]
                for (int i = 0; i < 4; i++)
                    ws[0x10 + i] ^= ws[0x14 + i];

                int rotCount = (ws[level] & 0x03) + 1;
                for (int r = 0; r < rotCount; r++)
                    RotateRightArray(ws, 0x10, 4);

                uint valA = Le32Load(ws, 0x10);
                uint valB = Le32Load(ws, 0x08);
                Le32Store(ws, 0x10, valA + valB);

                Array.Copy(ws, 0x00, ws, 0x14, 4);
                Array.Copy(ws, 0x10, ws, 0x00, 8);

                // Half-round B: uses ws[0x0C..0x0F]
                for (int i = 0; i < 4; i++)
                    ws[0x10 + i] ^= ws[0x14 + i];

                rotCount = (ws[level] & 0x03) + 1;
                for (int r = 0; r < rotCount; r++)
                    RotateRightArray(ws, 0x10, 4);

                valA = Le32Load(ws, 0x10);
                valB = Le32Load(ws, 0x0C);
                Le32Store(ws, 0x10, valA + valB);

                Array.Copy(ws, 0x00, ws, 0x14, 4);
                Array.Copy(ws, 0x10, ws, 0x00, 8);
            }

            // Step 7: Reverse-permute ws[0x00..0x07] -> output key
            outKey[0] = ws[3];
            outKey[1] = ws[5];
            outKey[2] = ws[6];
            outKey[3] = ws[1];
            outKey[4] = ws[0];
            outKey[5] = ws[7];
            outKey[6] = ws[4];
            outKey[7] = ws[2];

            return true;
        }

        public override string GetProviderName()
        {
            return "IC204";
        }
    }
}
