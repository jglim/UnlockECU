using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnlockECU
{
    /// <summary>
    /// Generic XOR algo, where the seed/xor key/output are of equal length, and output = (seed ^ xor key) 
    /// </summary>
    class XorAlgo : SecurityProvider
    {
        public override bool GenerateKey(byte[] inSeed, byte[] outKey, int accessLevel, List<Parameter> parameters)
        {
            byte[] xorKey = GetParameterBytearray(parameters, "K");

            if ((inSeed.Length != outKey.Length) || (inSeed.Length != xorKey.Length))
            {
                return false;
            }

            for (int i = 0; i < inSeed.Length; i++) 
            {
                outKey[i] = (byte)(inSeed[i] ^ xorKey[i]);
            }
            return true;
        }
        public override string GetProviderName()
        {
            return "XorAlgo";
        }
    }
}
