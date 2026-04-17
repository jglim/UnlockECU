using System;
using System.Collections.Generic;

namespace UnlockECU
{
    /// <summary>
    /// Implementation of extended VGS Algo with different keys for multiplication and xor
    /// </summary>
    class VGSSecurityAlgoExt : SecurityProvider
    {
        public override bool GenerateKey(byte[] inSeed, byte[] outKey, int accessLevel, List<Parameter> parameters)
        {
            byte[] cryptoKeyMultBytes = GetParameterBytearray(parameters, "M");
            uint cryptoKeyMult = BytesToInt(cryptoKeyMultBytes, Endian.Big);

            byte[] cryptoKeyXorBytes = GetParameterBytearray(parameters, "X");
            uint cryptoKeyXor = BytesToInt(cryptoKeyXorBytes, Endian.Big);

            if ((inSeed.Length != 4) || (outKey.Length != 4))
            {
                return false;
            }

            long seed = BytesToInt(inSeed, Endian.Big);
            long seedKey = cryptoKeyMult * (cryptoKeyXor ^ seed);

            IntToBytes((uint)seedKey, outKey, Endian.Big);
            return true;
        }
        public override string GetProviderName()
        {
            return "VGSSecurityAlgoExt";
        }
    }
}
