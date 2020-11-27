using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnlockECU
{
    /// <summary>
    /// Derivative of DaimlerStandardSecurityAlgo, with defined kA, kC constants for the intermediate transformation.
    /// </summary>
    class DaimlerStandardSecurityAlgoMod : SecurityProvider
    {
        public override bool GenerateKey(byte[] inSeed, byte[] outKey, int accessLevel, List<Parameter> parameters)
        {
            byte[] cryptoKeyBytes = GetParameterBytearray(parameters, "K");
            uint cryptoKey = BytesToInt(cryptoKeyBytes, Endian.Big);

            long kA = GetParameterLong(parameters, "kA");
            long kC = GetParameterLong(parameters, "kC");

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
            return "DaimlerStandardSecurityAlgoMod";
        }
    }
}
