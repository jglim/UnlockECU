using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnlockECU
{
    /// <summary>
    /// SWSP177: behavior, param sizes don't match DLL
    /// </summary>
    class SWSP177 : SecurityProvider
    {
        public override bool GenerateKey(byte[] inSeed, byte[] outKey, int accessLevel, List<Parameter> parameters)
        {
            byte[] cryptoKeyBytes = GetParameterBytearray(parameters, "KeyConst");
            uint cryptoKey = BytesToInt(cryptoKeyBytes, Endian.Big);

            if ((inSeed.Length != 4) || (outKey.Length != 4))
            {
                return false;
            }

            ulong seedA = BytesToInt(inSeed, Endian.Big, 0);

            for (int i = 0; i < 35; i++) 
            {
                seedA = seedA >> 1 | (seedA & 1) * 0x80000000;
                seedA ^= cryptoKey;
            }

            ulong seedKey = seedA & 0xFFFFFFFF;

            IntToBytes((uint)seedKey, outKey, Endian.Big);
            return true;
        }

        public override string GetProviderName()
        {
            return "SWSP177";
        }
    }
}
