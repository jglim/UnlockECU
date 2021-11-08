using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnlockECU
{
    /// <summary>
    /// IC172 Level 7 (Experimental)
    /// Originally discovered by Sergey (@Feezex) at https://github.com/jglim/UnlockECU/issues/2#issuecomment-962647700
    /// Key pool, transposition table and suffix may be moved into db.json in the future if this algo is reused elsewhere
    /// </summary>
    class IC172Algo1 : SecurityProvider
    {
        public override bool GenerateKey(byte[] inSeed, byte[] outKey, int accessLevel, List<Parameter> parameters)
        {
            if ((inSeed.Length != 8) || (outKey.Length != 8))
            {
                return false;
            }

            byte[] seedInput = new byte[] { inSeed[0], inSeed[2], inSeed[4], inSeed[6] };
            seedInput = ExpandByteArrayToNibbles(seedInput);

            List<byte[]> keyPool = new List<byte[]>();
            keyPool.Add(new byte[] { 0xEF, 0xCD, 0xAB, 0x89, 0x67, 0x45, 0x23, 0x01 });
            keyPool.Add(new byte[] { 0x45, 0x67, 0x01, 0x23, 0xCD, 0xEF, 0x89, 0xAB });
            keyPool.Add(new byte[] { 0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF });
            keyPool.Add(new byte[] { 0x89, 0xAB, 0xCD, 0xEF, 0x01, 0x23, 0x45, 0x67 });
            keyPool.Add(new byte[] { 0x54, 0x76, 0x10, 0x32, 0xDC, 0xFE, 0x98, 0xBA });
            keyPool.Add(new byte[] { 0xEF, 0xCD, 0xAB, 0x89, 0x67, 0x45, 0x23, 0x01 });
            keyPool.Add(new byte[] { 0x89, 0xAB, 0xCD, 0xEF, 0x01, 0x23, 0x45, 0x67 });
            keyPool.Add(new byte[] { 0xBA, 0x98, 0xFE, 0xDC, 0x32, 0x10, 0x76, 0x54 });

            byte[] transpositionTable = new byte[] { 5, 2, 7, 4, 1, 6, 3, 0 };
            byte[] intermediateKey = new byte[8];

            for (int i = 0; i < transpositionTable.Length; i++)
            {
                intermediateKey[i] = ExpandByteArrayToNibbles(keyPool[i])[seedInput[transpositionTable[i]]];
            }

            byte[] suffix = { 0x57, 0x49, 0x4C, 0x59 };
            byte[] assembledKey = CollapseByteArrayFromNibbles(intermediateKey);
            Array.ConstrainedCopy(assembledKey, 0, outKey, 0, 4);
            Array.ConstrainedCopy(suffix, 0, outKey, 4, 4);

            return true;
        }

        byte[] ExpandByteArrayToNibbles(byte[] inputArray)
        {
            byte[] result = new byte[inputArray.Length * 2];
            for (int i = 0; i < inputArray.Length; i++)
            {
                result[i * 2] = (byte)((inputArray[i] >> 4) & 0xF);
                result[i * 2 + 1] = (byte)(inputArray[i] & 0xF);
            }
            return result;
        }

        byte[] CollapseByteArrayFromNibbles(byte[] inputArray)
        {
            if ((inputArray.Length % 2) != 0)
            {
                throw new Exception("Attempted to form a byte array from an odd-numbered set of nibbles.");
            }
            byte[] result = new byte[inputArray.Length / 2];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = (byte)((inputArray[i * 2] << 4) | (inputArray[i * 2 + 1]));
            }
            return result;
        }

        public override string GetProviderName()
        {
            return "IC172Algo1";
        }
    }
}
