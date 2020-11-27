using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnlockECU
{
    /// <summary>
    /// Simpler version of DaimlerStandardSecurityAlgo with custom kA, kC, and no blockB
    /// </summary>
    class DRVU_PROF : SecurityProvider
    {
        public override bool GenerateKey(byte[] inSeed, byte[] outKey, int accessLevel, List<Parameter> parameters)
        {
            byte[] cryptoKeyBytes = GetParameterBytearray(parameters, "KeyConst");
            uint cryptoKey = BytesToInt(cryptoKeyBytes, Endian.Big);

            long kA = 258028488L;
            long kC = 1583629211L;

            if ((inSeed.Length != 4) || (outKey.Length != 4))
            {
                return false;
            }

            long seedA = BytesToInt(inSeed, Endian.Big, 0);

            long intermediate1 = kA * seedA + kC;
            long seedKey = intermediate1 % cryptoKey;

            IntToBytes((uint)seedKey, outKey, Endian.Big);
            return true;
        }

        public override string GetProviderName()
        {
            return "DRVU_PROF";
        }
    }
}
