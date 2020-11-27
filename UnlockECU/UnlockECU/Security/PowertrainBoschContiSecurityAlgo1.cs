using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnlockECU
{
    /// <summary>
    /// Appears to be specific to MED97
    /// </summary>
    class PowertrainBoschContiSecurityAlgo1 : SecurityProvider
    {
        public override bool GenerateKey(byte[] inSeed, byte[] outKey, int accessLevel, List<Parameter> parameters)
        {
            byte[] ubTable = GetParameterBytearray(parameters, "ubTable");
            byte[] Mask = GetParameterBytearray(parameters, "Mask");

            if ((inSeed.Length != 2) || (outKey.Length != 2))
            {
                return false;
            }

            uint inSeedAsInt = inSeed[1] | ((uint)inSeed[0] << 8);
            uint MaskAsInt = Mask[1] | ((uint)Mask[0] << 8);

            uint swBit1 = (inSeedAsInt & MaskAsInt & 0x4000) >> 12;
            uint swBit2 = (inSeedAsInt & MaskAsInt & 0x200) >> 8;
            uint swBit3 = (inSeedAsInt & MaskAsInt & 0x100) >> 8;

            uint keyAsInt = ubTable[swBit1 | swBit2 | swBit3] * inSeedAsInt;

            outKey[0] = (byte)((keyAsInt >> 16) & 0xFF);
            outKey[1] = (byte)((keyAsInt >> 8) & 0xFF);

            return true;
        }
        public override string GetProviderName()
        {
            return "PowertrainBoschContiSecurityAlgo1";
        }
    }
}
