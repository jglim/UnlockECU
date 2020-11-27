using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnlockECU
{
    /// <summary>
    /// Passes the input seed as the output key (4 bytes)
    /// </summary>
    class RBTM : SecurityProvider
    {
        public override bool GenerateKey(byte[] inSeed, byte[] outKey, int accessLevel, List<Parameter> parameters)
        {
            if ((inSeed.Length != 4) || (outKey.Length != 4))
            {
                return false;
            }

            outKey[0] = inSeed[0];
            outKey[1] = inSeed[1];
            outKey[2] = inSeed[2];
            outKey[3] = inSeed[3];
            return true;
        }

        public override string GetProviderName()
        {
            return "RBTM";
        }
    }
}
