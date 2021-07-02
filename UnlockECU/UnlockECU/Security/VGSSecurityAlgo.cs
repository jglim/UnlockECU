using System;
using System.Collections.Generic;

namespace UnlockECU
{
    /// <summary>
    /// Implementation of VGS Algo from VGSNAG2
    /// No formal name was found for this provider and it may be renamed in the future.
    /// </summary>
    class VGSSecurityAlgo : SecurityProvider
    {
        public override bool GenerateKey(byte[] inSeed, byte[] outKey, int accessLevel, List<Parameter> parameters)
        {
            byte[] cryptoKeyBytes = GetParameterBytearray(parameters, "K");
            uint cryptoKey = BytesToInt(cryptoKeyBytes, Endian.Big);

            if ((inSeed.Length != 4) || (outKey.Length != 4))
            {
                return false;
            }

            long seed = BytesToInt(inSeed, Endian.Big);
            long seedKey = cryptoKey * (cryptoKey ^ seed);

            IntToBytes((uint)seedKey, outKey, Endian.Big);
            return true;
        }
        public override string GetProviderName()
        {
            return "VGSSecurityAlgo";
        }
    }
}
