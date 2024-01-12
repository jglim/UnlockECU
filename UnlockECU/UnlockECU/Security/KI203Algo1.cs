using System.Collections.Generic;

namespace UnlockECU
{
    /// <summary>
    /// KI203Algo1 : https://github.com/jglim/UnlockECU/issues/30
    /// Collaborative effort by @rumator, Sergey (@Feezex) and Vladyslav Lupashevskyi (@VladLupashevskyi)
    /// This implementation is the original variant as reversed by @VladLupashevskyi using a firmware dump from @rumator,
    /// and it is preferred as the root keys can be directly used from a firmware dump.
    /// The root keys are often found in the firmware as a 7-element array of 32-bit integers, one for each level
    /// https://github.com/jglim/UnlockECU/issues/30#issuecomment-1881151971
    /// </summary>
    class KI203Algo1 : SecurityProvider
    {
        public override bool GenerateKey(byte[] inSeed, byte[] outKey, int accessLevel, List<Parameter> parameters)
        {
            if ((inSeed.Length != 4) || (outKey.Length != 4))
            {
                return false;
            }

            uint root = BytesToInt(GetParameterBytearray(parameters, "K"), Endian.Big);
            int ones = CountOnes(root);

            uint val = (uint)(
                    (inSeed[2] << 0) |
                    (inSeed[0] << 8) |
                    (inSeed[3] << 16) |
                    (inSeed[1] << 24)
                );

            val = RotateLeft(val, 3);
            val ^= root;
            val = RotateRight(val, ones);

            outKey[0] = GetByte(val, 0);
            outKey[1] = GetByte(val, 2);
            outKey[2] = GetByte(val, 3);
            outKey[3] = GetByte(val, 1);
            return true;
        }

        /// <summary>
        /// Condensed version of the original algo by @Feezex, where the shifts and transpositions
        /// have been folded into the root key, and only a single transpose op is applied once before the xor
        /// This implementation is here for reference and is not directly used by the application
        /// </summary>
        private static void CondensedGenerateKey() 
        {
            byte[] input = new byte[] { 0x3B, 0x1C, 0x0D, 0xDD };
            byte[] root = new byte[] { 0xF8, 0x20, 0x5A, 0x4F };

            byte[] result = new byte[4]
            {
                (byte)(((input[3] & 0xF) << 4) | (input[0] >> 4)),
                (byte)(((input[2] & 0xF) << 4) | (input[1] >> 4)),
                (byte)(((input[0] & 0xF) << 4) | (input[2] >> 4)),
                (byte)(((input[1] & 0xF) << 4) | (input[3] >> 4))
            };

            for (int i = 0; i < root.Length; i++)
            {
                result[i] ^= root[i];
            }
            // Expects result to be 2BF1EA82
        }

        /// <summary>
        /// Converts a condensed key into a original key
        /// This implementation is here for reference and is not directly used by the application
        /// </summary>
        static uint ConvertCondensedKeyToOriginal(uint val)
        {
            // Shared by @Feezex in https://github.com/jglim/UnlockECU/issues/30#issuecomment-1881815230
            // 203XXXXXXX_0223: 0x758A9A61 -> 30BACD45
            // 203XXXXXXX_0287: 0xF8205A4F-> 27FC2D10
            // 203XXXXXXX_0290: 0x054EDE92-> 4902EF27
            uint prekey =
            (0xFF00_0000 & (val << 16)) |
            (0x00FF_0000 & (val)) |
            (0x0000_FF00 & (val << 8)) |
            (0x0000_00FF & (val >> 24));
            return RotateLeft(prekey, CountOnes(prekey));
        }

        public override string GetProviderName()
        {
            return "KI203Algo1";
        }
    }
}

