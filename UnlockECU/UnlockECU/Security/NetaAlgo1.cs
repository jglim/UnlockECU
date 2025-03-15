using System.Collections.Generic;

namespace UnlockECU
{
    /// <summary>
    /// NetaAlgo1 
    /// Contribution by Ivan Beck @Ivanbk : https://github.com/jglim/UnlockECU/issues/50
    /// </summary>
    class NetaAlgo1 : SecurityProvider
    {
        public override bool GenerateKey(byte[] inSeed, byte[] outKey, int accessLevel, List<Parameter> parameters)
        {
            if (outKey.Length != 2) { return false; }
            if (inSeed.Length != 2) { return false; }
            byte[] pinBytes = GetParameterBytearray(parameters, "Pin");
            long v = pinBytes[1] | pinBytes[0] << 8;

            foreach (var challengeByte in inSeed)
            {
                v ^= (challengeByte << 8);
                for (int i = 0; i < 8; i++)
                {
                    if ((v & 0x8000) == 0)
                    {
                        v = (v << 1) & 0xFFFF;
                    }
                    else if ((v & 0x80) == 0)
                    {
                        v = ((v << 1) ^ 0x8025) & 0xFFFF;
                    }
                    else
                    {
                        v = ((v << 1) ^ 0x8408) & 0xFFFF;
                    }
                }
            }
            outKey[0] = (byte)(v >> 8);
            outKey[1] = (byte)v;
            return true;
        }

        public override string GetProviderName()
        {
            return "NetaAlgo1";
        }
    }
}