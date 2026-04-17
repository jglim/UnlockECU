using System;
using System.Collections.Generic;

namespace UnlockECU
{
    /// <summary>
    /// Implementation of VGSSecurityAlgo for 2 bytes seed/key pairs
    /// </summary>
    class VGSSecurityAlgo2Bytes : SecurityProvider
    {
        public override bool GenerateKey(byte[] inSeed, byte[] outKey, int accessLevel, List<Parameter> parameters)
        {
            byte[] cryptoKeyBytes = GetParameterBytearray(parameters, "K");
            uint cryptoKey = cryptoKeyBytes[1] | ((uint)cryptoKeyBytes[0]) << 8;
            if ((inSeed.Length != 2) || (outKey.Length != 2))
            {
                return false;
            }

            uint seed = inSeed[0] | ((uint)inSeed[1]) << 8;
            uint seedKey = cryptoKey * (cryptoKey ^ seed);

            outKey[0] = (byte)(seedKey >> 8);
            outKey[1] = (byte)seedKey;
            return true;
        }
        public override string GetProviderName()
        {
            return "VGSSecurityAlgo2Bytes";
        }
    }
}
