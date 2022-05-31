using System.Collections.Generic;

namespace UnlockECU
{
    /// <summary>
    /// ORC166 
    /// Contribution by Florian Pradines @Flo354 : https://github.com/jglim/UnlockECU/issues/18
    /// [!] Not verified with a known seed/key pair yet
    /// </summary>
    class ORC166 : SecurityProvider
    {
        public override bool GenerateKey(byte[] inSeed, byte[] outKey, int accessLevel, List<Parameter> parameters)
        {
            byte[] cryptoKeyBytes = GetParameterBytearray(parameters, "staticKey");
            uint cryptoKey = BytesToInt(cryptoKeyBytes, Endian.Big);

            if ((inSeed.Length != 8) || (outKey.Length != 4))
            {
                return false;
            }

            long i0 = (inSeed[0] >> 1) + (inSeed[1] << 1) + (inSeed[2] >> 2) + (inSeed[3] << 2);
            long i1 = (inSeed[0] << 1) + (inSeed[1] >> 1) + (inSeed[2] << 2) + (inSeed[3] >> 2);
            long i2 = (inSeed[0] >> 2) + (inSeed[1] << 2) + (inSeed[2] >> 1) + (inSeed[3] << 1);
            long i3 = (inSeed[0] << 2) + (inSeed[1] >> 2) + (inSeed[2] << 1) + (inSeed[3] >> 1);

            i0 = (i0 & 0xFF) << 24;
            i1 = (i1 & 0xFF) << 16;
            i2 = (i2 & 0xFF) << 8;
            i3 = (i3 & 0xFF) << 0;

            long seedKey = cryptoKey + i0 + i1 + i2 + i3;

            IntToBytes((uint)seedKey, outKey, Endian.Big);
            return true;
        }

        public override string GetProviderName()
        {
            return "ORC166";
        }
    }
}