using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnlockECU.Security
{
    /// <summary>
    /// HondaAlgo1
    /// Level 1, used for firmware flashing
    /// Keys are embedded as a 12-byte ascii block in the *.rwd.gz firmware files (J2534Standard/CalibFiles)
    /// Firmware format is documented in rwd-xray https://github.com/jpancotti/rwd-xray
    /// Key generation is documented in HondaReflashTool https://github.com/bouletmarc/HondaReflashTool
    /// </summary>
    class HondaAlgo1 : SecurityProvider
    {
        public override bool GenerateKey(byte[] inSeed, byte[] outKey, int accessLevel, List<Parameter> parameters)
        {
            if ((inSeed.Length != 2) || (outKey.Length != 2))
            {
                return false;
            }
            byte[] k = GetParameterBytearray(parameters, "K");

            ushort k0_xor = (ushort)(k[0] << 8 | k[1]);
            ushort k1_mul = (ushort)(k[2] << 8 | k[3]);
            ushort k2_mod = (ushort)(k[4] << 8 | k[5]);
            ushort seed = (ushort)(inSeed[0] << 8 | inSeed[1]);

            int key = seed * k1_mul;
            if (k2_mod != 0)
            {
                key %= k2_mod;
            }
            key ^= k0_xor + seed;
            key &= 0xFFFF;

            outKey[0] = (byte)(key >> 8);
            outKey[1] = (byte)key;
            return true;
        }

        public override string GetProviderName()
        {
            return "HondaAlgo1";
        }
    }
}
