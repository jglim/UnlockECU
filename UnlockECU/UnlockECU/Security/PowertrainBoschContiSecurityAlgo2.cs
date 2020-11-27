using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnlockECU
{
    /// <summary>
    /// Appears to be specific to MED97, SIM271CNG906
    /// </summary>
    class PowertrainBoschContiSecurityAlgo2 : SecurityProvider
    {
        public override bool GenerateKey(byte[] inSeed, byte[] outKey, int accessLevel, List<Parameter> parameters)
        {
            byte[] Table = GetParameterBytearray(parameters, "Table");
            byte[] uwMasc = GetParameterBytearray(parameters, "uwMasc");

            if ((inSeed.Length != 2) || (outKey.Length != 2))
            {
                return false;
            }

            uint shiftIndexA = 1;
            uint shiftIndexB = 1;
            uint activatedBits = 0;

            uint inSeedAsInt = inSeed[1] | ((uint)inSeed[0] << 8);
            uint uwMascAsInt = uwMasc[1] | ((uint)uwMasc[0] << 8);

            for (int i = 0; i < 16; i++)
            {
                if ((shiftIndexA & uwMascAsInt) > 0)
                {
                    if ((shiftIndexA & inSeedAsInt) > 0)
                    {
                        activatedBits |= shiftIndexB;
                    }
                    shiftIndexB *= 2;
                }
                shiftIndexA *= 2;
            }

            uint keyAsInt = (Table[activatedBits] * inSeedAsInt) >> 8;
            keyAsInt &= 0xFFFF;

            outKey[0] = (byte)((keyAsInt >> 8) & 0xFF);
            outKey[1] = (byte)((keyAsInt >> 0) & 0xFF);

            return true;
        }
        public override string GetProviderName()
        {
            return "PowertrainBoschContiSecurityAlgo2";
        }
    }
}
