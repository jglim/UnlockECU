using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnlockECU
{
    /// <summary>
    /// IC172 Level 113 (Experimental)
    /// Originally discovered by Sergey (@Feezex) at https://github.com/jglim/UnlockECU/issues/2#issuecomment-962647700
    /// Currently at a second revision that addresses a bug at https://github.com/jglim/UnlockECU/issues/2#issuecomment-1028035906
    /// Output samples have been verified with a recently released generator by Sergey Klenov (thank you!)
    /// </summary>
    class IC172Algo2 : SecurityProvider
    {
        public override bool GenerateKey(byte[] inSeed, byte[] outKey, int accessLevel, List<Parameter> parameters)
        {
            if ((inSeed.Length != 4) || (outKey.Length != 4))
            {
                return false;
            }

            List<int[]> keyPool = new();
            keyPool.Add(new int[] { 0,   -1,  -2,  -3,  -4,  -5,  -6,  -7,  -8,  -9,  -10, -11, -12, -13, -14, -15 });
            keyPool.Add(new int[] { 251, 252, 253, 254, 255, 256, 257, 258, 243, 244, 245, 246, 247, 248, 249, 250 });
            keyPool.Add(new int[] { 0,   1,   -2,  -1,  4,   5,   2,   3,   8,   9,   6,   7,   12,  13,  10,  11  });
            keyPool.Add(new int[] { 73,  72,  75,  74,  69,  68,  71,  70,  81,  80,  83,  82,  77,  76,  79,  78  });
            keyPool.Add(new int[] { 0,  -1,   -2,  -3,  4,   3,   2,   1,   8,   7,   6,   5,   12,  11,  10,  9,  });
            keyPool.Add(new int[] { 203, 202, 205, 204, 207, 206, 209, 208, 195, 194, 197, 196, 199, 198, 201, 200 });
            keyPool.Add(new int[] { 0,   1,   2,   3,   -4,  -3,  -2,  -1,  8,   9,   10,  11,  4,   5,   6,   7   });
            keyPool.Add(new int[] { 185, 184, 183, 182, 181, 180, 179, 178, 193, 192, 191, 190, 189, 188, 187, 186 });

            byte[] nibbles = ExpandByteArrayToNibbles(inSeed);
            int key_result = 0;
            for (int i = 0; i < 8; i++)
            {
                key_result += keyPool[i][nibbles[i]] << ((7 - i) * 4);
            }

            IntToBytes((uint)key_result, outKey, Endian.Big);
            return true;
        }

        public override string GetProviderName()
        {
            return "IC172Algo2";
        }
    }
}
