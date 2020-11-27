using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnlockECU
{
    /// <summary>
    /// Derivative of DaimlerStandardSecurityAlgoMod, with 4 custom values (kA0, kA1, kC0, kC1) for the intermediate transformation.
    /// </summary>
    class DaimlerStandardSecurityAlgoRefG : SecurityProvider
    {
        public override bool GenerateKey(byte[] inSeed, byte[] outKey, int accessLevel, List<Parameter> parameters)
        {
            byte[] cryptoKeyBytes = GetParameterBytearray(parameters, "K_refG");
            uint cryptoKey = BytesToInt(cryptoKeyBytes, Endian.Big);

            long kA_0 = 3040238857L;
            long kA_1 = 4126034881L;
            long kC_0 = 2094854071L;
            long kC_1 = 3555108353L;

            if ((inSeed.Length != 8) || (outKey.Length != 4))
            {
                return false;
            }

            long seedA = BytesToInt(inSeed, Endian.Big, 0);
            long seedB = BytesToInt(inSeed, Endian.Big, 4);

            long intermediate1 = kA_0 * seedA + kC_0;
            long intermediate2 = kA_1 * seedB + kC_1;
            long seedKey = intermediate1 ^ intermediate2 ^ cryptoKey;

            IntToBytes((uint)seedKey, outKey, Endian.Big);
            return true;
        }

        public override string GetProviderName()
        {
            return "DaimlerStandardSecurityAlgoRefG";
        }
    }
}
