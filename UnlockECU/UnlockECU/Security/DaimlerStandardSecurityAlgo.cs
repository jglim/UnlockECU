using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnlockECU
{
    /// <summary>
    /// Basic implementation DaimlerStandardSecurityAlgo, with hardcoded kA, kC constants for the intermediate transformation.
    /// </summary>
    class DaimlerStandardSecurityAlgo : SecurityProvider
    {
        public override bool GenerateKey(byte[] inSeed, byte[] outKey, int accessLevel, List<Parameter> parameters)
        {
            byte[] cryptoKeyBytes = GetParameterBytearray(parameters, "K");
            uint cryptoKey = BytesToInt(cryptoKeyBytes, Endian.Big);

            long kA = 1103515245L;
            long kC = 12345L;

            if ((inSeed.Length != 8) || (outKey.Length != 4))
            {
                return false;
            }

            long seedA = BytesToInt(inSeed, Endian.Big, 0);
            long seedB = BytesToInt(inSeed, Endian.Big, 4);

            long intermediate1 = kA * seedA + kC;
            long intermediate2 = kA * seedB + kC;
            long seedKey = intermediate1 ^ intermediate2 ^ cryptoKey;

            IntToBytes((uint)seedKey, outKey, Endian.Big);
            return true;
        }
        public override string GetProviderName()
        {
            return "DaimlerStandardSecurityAlgo";
        }
    }
}
