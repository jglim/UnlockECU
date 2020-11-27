using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace UnlockECU
{
    class Program
    {

		static void Main(string[] args)
        {
            Console.WriteLine("UnlockECU (Running as console application)");

			string definitionJson = File.ReadAllText("db.json");

            List<Definition> definitions = System.Text.Json.JsonSerializer.Deserialize<List<Definition>>(definitionJson);
            List<SecurityProvider> providers = SecurityProvider.GetSecurityProviders();

            ReverseKey(BitUtility.BytesFromHex("BA00D268 972D452D"), BitUtility.BytesFromHex("BE515D46")); // reverse key 0x3F9C71A5 (CRD3S2SEC9A)

            Console.ReadKey();
        }

        static void ReverseKey(byte[] inSeed, byte[] outKeyBytes) 
        {
            long kA = 1103515245L;
            long kC = 12345L;

            long seedA = BytesToInt(inSeed, Endian.Big, 0);
            long seedB = BytesToInt(inSeed, Endian.Big, 4);

            long outKey = BytesToInt(outKeyBytes, Endian.Big);

            long intermediate1 = kA * seedA + kC;
            long intermediate2 = kA * seedB + kC;

            long xorA = intermediate1 ^ intermediate2;

            long reverseCryptoKey = (xorA ^ outKey) & 0xFFFFFFFF; // reverse key 0x3F9C71A5 (CRD3S2SEC9A)

            Console.WriteLine($"Reversed DSSA key: {reverseCryptoKey:X}");
        }

        public enum Endian
        {
            Big,
            Little,
        }
        public static uint BytesToInt(byte[] inBytes, Endian endian, int offset = 0)
        {
            uint result = 0;
            if (endian == Endian.Big)
            {
                result |= (uint)inBytes[offset++] << 24;
                result |= (uint)inBytes[offset++] << 16;
                result |= (uint)inBytes[offset++] << 8;
                result |= (uint)inBytes[offset++] << 0;
            }
            else
            {
                result |= (uint)inBytes[offset++] << 0;
                result |= (uint)inBytes[offset++] << 8;
                result |= (uint)inBytes[offset++] << 16;
                result |= (uint)inBytes[offset++] << 24;
            }
            return result;
        }
    }
}
