using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnlockECU
{
    class RenaultRadio : SecurityProvider
    {
        public override bool GenerateKey(byte[] inSeed, byte[] outKey, int accessLevel, List<Parameter> parameters)
        {
            if (inSeed == null || outKey == null)
                return false;

            // Expecting 4 bytes (HEX: 59 30 32 30)
            if (inSeed.Length < 4)
                return false;

            // First 4 bytes as ASCII
            char c0 = (char)inSeed[0];
            char c1 = (char)inSeed[1];
            char c2 = (char)inSeed[2];
            char c3 = (char)inSeed[3];

            int var0 = char.ToUpper(c0);
            int var1 = c1;
            int var2 = c2;
            int var3 = c3;

            var0 = var0 * 5;
            var1 = var0 * 2 + var1 - 698;
            var2 = (var2 * 5) * 2 + var1;
            var3 = var3 + var2 - 528;

            int sum = ((var3 << 3) - var3) % 100;

            if (sum < 0)
                sum += 100;

            int call = sum / 10;
            int remainder = (sum % 10) * 5;

            int varf = remainder * 2 + call;

            if (var1 == 0)
                return false;

            int eax = 259 % var1 % 100;
            eax = eax * 5;
            int edx = eax * 5;
            eax = edx * 4 + varf;

            // OUTPUT (2 bytes Big Endian)
            if (outKey.Length < 2)
                return false;

            outKey[0] = (byte)((eax >> 8) & 0xFF);
            outKey[1] = (byte)(eax & 0xFF);

            return true;
        }

        public override string GetProviderName()
        {
            return "RenaultRadio";
        }
    }
}
