using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnlockECU
{
    /// <summary>
    /// Simpler version of DaimlerStandardSecurityAlgo with custom kA, kC, and no blockB. 
    /// Similar to DRVU_PROF, with different initial parameter data types
    /// </summary>
    class EDIFF290 : SecurityProvider
    {
        public override bool GenerateKey(byte[] inSeed, byte[] outKey, int accessLevel, List<Parameter> parameters)
        {
            byte[] cryptoKeyBytes = GetParameterBytearray(parameters, "KeyK");
            uint cryptoKey = BytesToInt(cryptoKeyBytes, Endian.Big);

            long kA = GetParameterInteger(parameters, "kA");
            long kC = GetParameterInteger(parameters, "kC");

            if ((inSeed.Length != 8) || (outKey.Length != 4))
            {
                return false;
            }

            long seedA = BytesToInt(inSeed, Endian.Big, 0);

            // kA * seedA + kC, but constrained to 32bits
            long intermediate1 = kA * seedA;
            intermediate1 &= 0xFFFFFFFF;
            intermediate1 += kC;
            intermediate1 &= 0xFFFFFFFF;

            long seedKey = intermediate1 % cryptoKey;

            IntToBytes((uint)seedKey, outKey, Endian.Big);
            return true;
        }

        public override string GetProviderName()
        {
            return "EDIFF290";
        }
    }
}
