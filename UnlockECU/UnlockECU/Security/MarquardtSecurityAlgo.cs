using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnlockECU
{
    class MarquardtSecurityAlgo : SecurityProvider
    {
        public override bool GenerateKey(byte[] inSeed, byte[] outKey, int accessLevel, List<Parameter> parameters)
        {
            byte[] constMBytes = GetParameterBytearray(parameters, "const_M");
            byte[] constCBytes = GetParameterBytearray(parameters, "const_C");
            byte[] constABytes = GetParameterBytearray(parameters, "const_A");

            uint constM = BytesToInt(constMBytes, Endian.Big); // 228
            uint constC = BytesToInt(constCBytes, Endian.Big); // 236
            uint constA = BytesToInt(constABytes, Endian.Big); // 232
            uint inSeedAsInt = BytesToInt(inSeed, Endian.Big); // 232

            if ((inSeed.Length != 4) || (outKey.Length != 4))
            {
                return false;
            }

            uint outKeyInt;
            unchecked 
            {
                outKeyInt = constC + inSeedAsInt * constA % constM;
            }

            IntToBytes(outKeyInt, outKey, Endian.Big);

            return true;
        }
        public override string GetProviderName()
        {
            return "MarquardtSecurityAlgo";
        }
    }
}
