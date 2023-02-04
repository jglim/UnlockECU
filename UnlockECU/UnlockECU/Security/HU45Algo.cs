using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnlockECU
{
    /// <summary>
    /// HU45Algo as found in HU45_hu45_sec_12_05_01, Intended for level 7 (CBF)
    /// No algo name was formally specified, this name is unofficial
    /// </summary>
    class HU45Algo : SecurityProvider
    {
        public override bool GenerateKey(byte[] inSeed, byte[] outKey, int accessLevel, List<Parameter> parameters)
        {
            if ((inSeed.Length != 2) || (outKey.Length != 2))
            {
                return false;
            }
            // Levels, input and output length are not checked
            // Only 2 bytes are checked and modified
            int partial = ~(inSeed[0] + (inSeed[1] << 8)) - 0x307;
            outKey[0] = (byte)(partial >> 8);
            outKey[1] = (byte)(~inSeed[0] - 7);
            return true;
        }

        public override string GetProviderName()
        {
            return "HU45Algo";
        }
    }
}
