using System;
using System.Collections.Generic;

namespace UnlockECU
{
    /// <summary>
    /// Generic array reverse algo, where the seed/output are of equal length, and output is the reversed seed array
    /// </summary>
    class ArrayReverseAlgo : SecurityProvider
    {
        public override bool GenerateKey(byte[] inSeed, byte[] outKey, int accessLevel, List<Parameter> parameters)
        {
            if (inSeed.Length != outKey.Length)
            {
                return false;
            }
            Array.Reverse(inSeed);
            Array.Copy(inSeed, outKey, inSeed.Length);
            return true;
        }
        public override string GetProviderName()
        {
            return "ArrayReverseAlgo";
        }
    }
}

