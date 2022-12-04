using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnlockECU
{    /// <summary>
     /// KIAlgo1
     /// Contribution by Vladyslav Lupashevskyi @VladLupashevskyi : https://github.com/jglim/UnlockECU/issues/12
     /// Rust-based reference implementation is available from @VladLupashevskyi at https://github.com/VladLupashevskyi/ki211-seed-key
     /// </summary>
    class KIAlgo1 : SecurityProvider
    {
        public override bool GenerateKey(byte[] inSeed, byte[] outKey, int accessLevel, List<Parameter> parameters)
        {
            byte level = GetParameterByte(parameters, "Level");
            byte[] rootBytes = GetParameterBytearray(parameters, "K");
            uint rootRotated = BytesToInt(rootBytes, Endian.Big);

            if ((inSeed.Length != 8) || (outKey.Length != 7))
            {
                return false;
            }

            uint seedRegrouped = BytesToInt(new byte[] { inSeed[2], inSeed[6], inSeed[0], inSeed[4] }, Endian.Little);
            int rotationCount = (int)(seedRegrouped & 7) + 2;

            for (int i = 0; i < rotationCount; i++)
            {
                bool msbSet = BitSet(rootRotated, 0)
                     ^ BitSet(rootRotated, 7)
                     ^ BitSet(rootRotated, 17)
                     ^ BitSet(rootRotated, 26);
                rootRotated &= 0x7FFFFFFF;
                rootRotated |= msbSet ? 0x80000000 : 0;
                rootRotated = RotateRight(rootRotated, 1);
            }

            ushort seedHigh = (ushort)(seedRegrouped >> 16);
            ushort seedLow = (ushort)seedRegrouped;

            for (int i = 0; i < 2; i++)
            {
                seedHigh ^= seedLow;
                seedHigh = RotateLeft(seedHigh, rotationCount);
                ushort tangled = (ushort)(seedHigh + (ushort)rootRotated);
                rootRotated >>= 16;
                seedHigh = seedLow;
                seedLow = tangled;
            }

            byte[] prefixBytes = new byte[4];
            uint prefix = (uint)(seedLow | (seedHigh << 16));
            IntToBytes(prefix, prefixBytes, Endian.Little);

            outKey[1] = prefixBytes[0];
            outKey[2] = prefixBytes[1];
            outKey[3] = prefixBytes[3];
            outKey[4] = prefixBytes[2];

            // Originally documented as 'access level'
            outKey[0] = level;

            // VCI fingerprint
            outKey[5] = 0xFF;
            outKey[6] = 0xFF;

            return true;
        }

        static bool BitSet(uint input, int bit)
        {
            return (input & (1 << bit)) > 0;
        }

        static ushort RotateLeft(ushort input, int rotations)
        {
            for (int i = 0; i < rotations; i++)
            {
                if ((input & 0x8000) == 0)
                {
                    input <<= 1;
                }
                else
                {
                    input <<= 1;
                    input |= 1;
                }
            }
            return input;
        }
        static uint RotateRight(uint input, int rotations)
        {
            for (int i = 0; i < rotations; i++)
            {
                if ((input & 1) == 0)
                {
                    input >>= 1;
                }
                else
                {
                    input >>= 1;
                    input |= 0x80000000;
                }
            }
            return input;
        }

        public override string GetProviderName()
        {
            return "KIAlgo1";
        }
    }
}