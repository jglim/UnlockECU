using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnlockECU
{
    /// <summary>
    /// Passes the input seed as the output key (2 bytes)
    /// </summary>
    class OCM172 : SecurityProvider
    {
        public override bool GenerateKey(byte[] inSeed, byte[] outKey, int accessLevel, List<Parameter> parameters)
        {
            if ((inSeed.Length != 2) || (outKey.Length != 2))
            {
                return false;
            }

            outKey[0] = inSeed[0];
            outKey[1] = inSeed[1];
            return true;
        }

        public override string GetProviderName()
        {
            return "OCM172";
        }
    }
}
