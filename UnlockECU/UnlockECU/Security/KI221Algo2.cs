using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnlockECU
{
    /// <summary>
    /// KI221Algo2
    /// Exists on firmware as a consistent V850E2 pattern: 
    /// 
    /// mov imm32, reg
    /// xor reg, dst
    /// mov imm32, reg2
    /// add reg2, dst
    /// 
    /// Only one set of values has been observed so far: (x ^ 0x78253947) + 0x83249272
    /// </summary>
    class KI221Algo2 : SecurityProvider
    {
        public override bool GenerateKey(byte[] inSeed, byte[] outKey, int accessLevel, List<Parameter> parameters)
        {
            if ((inSeed.Length != 4) || (outKey.Length != 4))
            {
                return false;
            }
            byte[] xorBytes = GetParameterBytearray(parameters, "Xor");
            byte[] addBytes = GetParameterBytearray(parameters, "Add");
            uint xor = BytesToInt(xorBytes, Endian.Little); // Almost always 0x78253947
            uint add = BytesToInt(addBytes, Endian.Little); // Almost always 0x83249272
            uint seed = BytesToInt(inSeed, Endian.Big);

            seed ^= xor;
            seed += add;

            IntToBytes(seed, outKey, Endian.Big);
            return true;
        }

        public override string GetProviderName()
        {
            return "KI221Algo2";
        }
    }
}
