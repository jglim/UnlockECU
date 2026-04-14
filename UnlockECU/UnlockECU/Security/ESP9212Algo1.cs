using System.Collections.Generic;

namespace UnlockECU
{
    /// <summary>
    /// ESP9212Algo1 
    /// Experimental, unverified
    /// </summary>
    class ESP9212Algo1 : SecurityProvider
    {
        public override bool GenerateKey(byte[] inSeed, byte[] outKey, int accessLevel, List<Parameter> parameters)
        {
            if (outKey.Length != 2) { return false; }
            if (inSeed.Length != 2) { return false; }

            uint val = inSeed[0];
            val <<= 8;
            val |= inSeed[1];

            uint snapshot = val;
            val /= 4;
            val ^= snapshot;
            val *= 8;
            val ^= snapshot;

            outKey[0] = (byte)(val >> 8);
            outKey[1] = (byte)val;
            return true;
        }

        public override string GetProviderName()
        {
            return "ESP9212Algo1";
        }
    }
}