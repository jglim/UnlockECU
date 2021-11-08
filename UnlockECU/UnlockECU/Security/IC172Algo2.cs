using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnlockECU
{
    /// <summary>
    /// IC172 Level 113 (Experimental)
    /// Originally discovered by Sergey (@Feezex) at https://github.com/jglim/UnlockECU/issues/2#issuecomment-962647700
    /// Preliminary and likely to be revised, though the generated output will be unaffected
    /// </summary>
    class IC172Algo2 : SecurityProvider
    {
        public override bool GenerateKey(byte[] inSeed, byte[] outKey, int accessLevel, List<Parameter> parameters)
        {
            if ((inSeed.Length != 4) || (outKey.Length != 4))
            {
                return false;
            }

            IntToBytes(GenSeedKeyHL(inSeed), outKey, Endian.Big);

            return true;
        }

        uint GenSeedKeyHL(byte[] seed_bytes)
        {
            uint sb3 = Convert.ToUInt32(seed_bytes[3]);
            uint sb3_h = sb3 / 0x10;
            uint sb3_l = sb3 - sb3_h * 0x10;

            uint CORR8 = 0;
            if (sb3_l > 7)
            {
                CORR8 = 0x10;
            }
            uint k8 = 185 - sb3_l + CORR8;

            uint CORR7 = 0;
            uint CORR72 = 0;

            if (sb3_h >= 4)
            {
                CORR7 = 0x80;
            }
            if (sb3_h >= 8)
            {
                CORR72 = 0x80;
            }
            if (sb3_h >= 0x0C)
            {
                CORR7 = 0x100;
            }

            uint k7 = (16 * sb3_h) - CORR7 + CORR72;
            uint sb2_h = (byte)(seed_bytes[2] >> 4);
            uint sb2_l = (byte)(seed_bytes[2] & 0xF);

            uint CORR61 = 0;
            uint CORR62 = 0;
            uint CORR63 = 0;
            uint CORR64 = 0;
            uint CORR65 = 0;
            uint CORR66 = 0;
            uint CORR67 = 0;
            uint CORR68 = 0;
            uint CORR69 = 0;
            uint CORR6A = 0;
            uint CORR6B = 0;
            uint CORR6C = 0;
            uint CORR6D = 0;
            uint CORR6E = 0;
            uint CORR6F = 0;

            if (sb2_l > 0)
            {
                CORR61 = 0x100;
            }
            if (sb2_l > 1)
            {
                CORR62 = 0x100 * 3;
            }
            if (sb2_l > 2)
            {
                CORR63 = 0x100;
            }
            if (sb2_l > 3)
            {
                CORR64 = 0x100 * 3;
            }
            if (sb2_l > 4)
            {
                CORR65 = 0x100;
            }
            if (sb2_l > 5)
            {
                CORR66 = 0x100 * 3;
            }
            if (sb2_l > 6)
            {
                CORR67 = 0x100;
            }
            if (sb2_l > 7)
            {
                CORR68 = 0xD00;
            }
            if (sb2_l > 8)
            {
                CORR69 = 0x100;
            }
            if (sb2_l > 9)
            {
                CORR6A = 0x100 * 3;
            }
            if (sb2_l > 0x0A)
            {
                CORR6B = 0x100;
            }
            if (sb2_l > 0x0B)
            {
                CORR6C = 0x100 * 3;
            }
            if (sb2_l > 0x0C)
            {
                CORR6D = 0x100;
            }
            if (sb2_l > 0x0D)
            {
                CORR6E = 0x100 * 3;
            }
            if (sb2_l > 0x0E)
            {
                CORR6F = 0x100;
            }
            uint k6 = 0xCB00 - CORR61 + CORR62 - CORR63 + CORR64 - CORR65 + CORR66 - CORR67 - CORR68 - CORR69 + CORR6A - CORR6B + CORR6C - CORR6D + CORR6E - CORR6F;

            //  6 done

            uint CORR5 = 0;
            uint CORR52 = 0;

            if (sb2_h >= 1)
            {
                CORR5 = 0x1000 * sb2_h;
            }
            if (sb2_h >= 4)
            {
                CORR52 = 0x1000 + 0x7000;
            }
            if (sb2_h >= 5)
            {
                CORR5 = 0x1000 * sb2_h;
            }
            if (sb2_h >= 8)
            {
                CORR52 = 0x7000 + 0x1000 * sb2_h + 0x1000;
            }
            if (sb2_h >= 9)
            {
                CORR5 = 0x1000 * sb2_h + 0x1000;
            }
            if (sb2_h >= 10)
            {
                CORR5 = 0x1000 * sb2_h + 0x1000 * 2;
            }
            if (sb2_h >= 11)
            {
                CORR5 = 0x1000 * sb2_h + 0x1000 * 3;
            }
            if (sb2_h >= 12)
            {
                CORR5 = 0x1000 * sb2_h - 0x1000 * 4;
            }
            if (sb2_h >= 13)
            {
                CORR5 = 0x1000 * sb2_h - 0x1000 * 3;
            }
            if (sb2_h >= 14)
            {
                CORR5 = 0x1000 * sb2_h - 0x1000 * 2;
            }
            if (sb2_h >= 15)
            {
                CORR5 = 0x1000 * sb2_h - 0x1000;
            }
            uint k5 = k6 - CORR5 + CORR52;

            // 5 done

            uint sb1_h = (byte)(seed_bytes[1] >> 4);
            uint sb1_l = (byte)(seed_bytes[1] & 0xF);

            uint CORR4 = 0;
            uint CORR41 = 0;
            if (sb1_l >= 1)
            {
                CORR4 = 0x10000;
            }
            if (sb1_l >= 2)
            {
                CORR41 = 0x10000 * 3;
            }
            if (sb1_l >= 3)
            {
                CORR41 = 0x10000 * (sb1_l - 1);
            }
            if (sb1_l >= 4)
            {
                CORR41 = 0x10000 * (sb1_l - 7);
            }
            if (sb1_l >= 5)
            {
                CORR41 = 0x10000 * (sb1_l - 9);
            }
            if (sb1_l >= 6)
            {
                CORR41 = 0x10000 * (sb1_l - 7);
            }
            if (sb1_l >= 7)
            {
                CORR41 = 0x10000 * (sb1_l - 9);
            }
            if (sb1_l >= 8)
            {
                CORR41 = 0x10000 * (sb1_l + 1);
            }
            if (sb1_l >= 9)
            {
                CORR41 = 0x10000 * (sb1_l - 1);
            }
            if (sb1_l >= 10)
            {
                CORR41 = 0x10000 * (sb1_l + 1);
            }
            if (sb1_l >= 11)
            {
                CORR41 = 0x10000 * (sb1_l - 1);
            }
            if (sb1_l >= 12)
            {
                CORR41 = 0x10000 * (sb1_l - 7);
            }
            if (sb1_l >= 13)
            {
                CORR41 = 0x10000 * (sb1_l - 9);
            }
            if (sb1_l >= 14)
            {
                CORR41 = 0x10000 * (sb1_l + 9);
            }
            if (sb1_l >= 15)
            {
                CORR41 = 0x10000 * (sb1_l - 9);
            }


            uint k4 = 0x490000 - CORR4 + CORR41;

            // 4 done

            uint CORR3 = 0;
            uint CORR31 = 0;
            if (sb1_h >= 1)
            {
                CORR31 = 1;
            }
            if (sb1_h >= 2)
            {
                CORR3 = (sb1_h + 1);
            }
            if (sb1_h >= 3)
            {
                CORR31 = (sb1_h);
            }
            if (sb1_h >= 4)
            {
                CORR31 = (sb1_h + 5);
            }
            if (sb1_h >= 5)
            {
                CORR31 = (sb1_h + 6);
            }
            if (sb1_h >= 6)
            {
                CORR3 = (sb1_h + 4);
            }
            if (sb1_h >= 7)
            {
                CORR31 = (sb1_h + 7);
            }
            if (sb1_h >= 8)
            {
                CORR31 = (sb1_h + 12);
            }
            if (sb1_h >= 9)
            {
                CORR31 = (sb1_h + 13);
            }
            if (sb1_h >= 10)
            {
                CORR3 = (sb1_h + 7);
            }
            if (sb1_h >= 11)
            {
                CORR31 = (sb1_h + 14);
            }
            if (sb1_h >= 12)
            {
                CORR31 = (sb1_h + 19);
            }
            if (sb1_h >= 13)
            {
                CORR31 = (sb1_h + 20);
            }
            if (sb1_h >= 14)
            {
                CORR3 = (sb1_h + 10);
            }
            if (sb1_h >= 15)
            {
                CORR31 = (sb1_h + 21);
            }
            CORR3 <<= 20; // 1048576
            CORR31 <<= 20;

            uint k3 = k4 - CORR3 + CORR31;

            // 3 done

            uint U1 = Convert.ToUInt32(seed_bytes[0]);
            uint sb0_h = (byte)(seed_bytes[0] >> 4);
            uint sb0_l = (byte)(seed_bytes[0] & 0xF);


            uint CORR2 = 0x1000000; // 1 << 24
            uint k2 = CORR2 * sb0_l;
            if (sb0_l >= 8)
            {
                k2 = CORR2 * (sb0_l - 16);
            }

            uint k1 = sb0_h << 28;

            uint constant_msb = 0xFB000000; // 0xFB << 24

            uint key_final = constant_msb + k8 + k7 + k5 + k3 + k2 - k1;

            return key_final;
        }

        public override string GetProviderName()
        {
            return "IC172Algo2";
        }
    }
}
