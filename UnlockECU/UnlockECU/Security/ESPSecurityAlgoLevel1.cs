using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnlockECU
{
    class ESPSecurityAlgoLevel1 : SecurityProvider
    {
        public override bool GenerateKey(byte[] inSeed, byte[] outKey, int accessLevel, List<Parameter> parameters)
        {
            if ((inSeed.Length != 2) || (outKey.Length != 2))
            {
                return false;
            }

            uint seedAsInt = inSeed[1] | ((uint)inSeed[0] << 8);
            uint key = 4 * ((seedAsInt >> 3) ^ seedAsInt) ^ seedAsInt;

            outKey[0] = (byte)(key >> 8);
            outKey[1] = (byte)(key >> 0);
            return true;
        }

        public override string GetProviderName()
        {
            return "ESPSecurityAlgoLevel1";
        }
    }
}
