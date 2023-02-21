using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnlockECU
{
    /// <summary>
    /// KI221Algo1
    /// This algo was embedded in KI221.CBF (accidentally?) as a compiled script, referenced near DJ_Displaylight_write in the SecurityAccess function
    /// When the SecurityAccess function is invoked correctly, it prints "Test1", sends 2701, reads the 6701 response, then sends a 2702 response
    /// That function requires a 1 byte access level and 1 byte padding as its function parameter. This value is copied verbatim to the 2702 response
    /// </summary>
    class KI221Algo1 : SecurityProvider
    {
        public override bool GenerateKey(byte[] inSeed, byte[] outKey, int accessLevel, List<Parameter> parameters)
        {
            if ((inSeed.Length != 8) || (outKey.Length != 7))
            {
                return false;
            }
            // In the original implementation, level is not used in the key computation
            byte level = GetParameterByte(parameters, "Level");
            byte[] k = GetParameterBytearray(parameters, "K");
            uint rootKey = BytesToInt(k, Endian.Big);

            k[0] ^= inSeed[6];
            k[1] ^= inSeed[4];
            k[2] ^= inSeed[2];
            k[3] ^= inSeed[0];

            uint rs = BytesToInt(new byte[] { k[0], k[3], k[2], k[1], }, Endian.Little);

            uint inter = ((rs << 29) + (rs >> 3)) ^ rootKey;
            uint key = (inter >> 25) + (inter << 7);
            IntToBytes(key, k, Endian.Big);

            // Last 2 bytes are originally hardcoded as zeroes, other tools use FF.
            // That value is probably discarded by the ECU and can be personalized (e.g. IC172Algo1)
            byte[] result = new byte[] { level, k[0], k[3], k[2], k[1], 0, 0 };
            Array.ConstrainedCopy(result, 0, outKey, 0, result.Length);

            return true;
        }

        public override string GetProviderName()
        {
            return "KI221Algo1";
        }
    }
}
