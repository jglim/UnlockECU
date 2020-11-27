using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnlockECU;

namespace UnlockECUTests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {

        }

        [Test]
        public void VerifyOutputWithOfficialDLLs()
        {
            string dbPath = $"{GetLibraryFolder()}db.json";
            string definitionJson = File.ReadAllText(dbPath);
            List<Definition> definitions = System.Text.Json.JsonSerializer.Deserialize<List<Definition>>(definitionJson);
            List<SecurityProvider> providers = SecurityProvider.GetSecurityProviders();

            QuickTest(definitions.Find(x => (x.EcuName == "CRD3S2SEC9A") && (x.AccessLevel == 9) && (x.Provider == "DaimlerStandardSecurityAlgo")), providers);
            QuickTest(definitions.Find(x => (x.EcuName == "RBS222") && (x.AccessLevel == 11) && (x.Provider == "DaimlerStandardSecurityAlgoMod")), providers);
            QuickTest(definitions.Find(x => (x.EcuName == "IC177") && (x.AccessLevel == 11) && (x.Provider == "DaimlerStandardSecurityAlgoRefG")), providers);
            QuickTest(definitions.Find(x => (x.EcuName == "MED40") && (x.AccessLevel == 5) && (x.Provider == "PowertrainSecurityAlgo")), providers);
            QuickTest(definitions.Find(x => (x.EcuName == "CR6NFZ") && (x.AccessLevel == 1) && (x.Provider == "PowertrainSecurityAlgo2")), providers);
            QuickTest(definitions.Find(x => (x.EcuName == "DCDC223") && (x.AccessLevel == 17) && (x.Provider == "EsLibEd25519")), providers);

            Assert.Pass();
        }

        static string GetLibraryFolder() 
        {
            return $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}{Path.DirectorySeparatorChar}Library{Path.DirectorySeparatorChar}";
        }

        static void QuickTest(Definition definition, List<SecurityProvider> providers)
        {
            // Build in x86 to test, else the pinvokes will fail!

            SecurityProvider provider = providers.Find(x => x.GetProviderName() == definition.Provider);
            if (provider is null)
            {
                Console.WriteLine($"Could not find a security provider for {definition.EcuName} ({definition.Provider})");
                Assert.Fail();
                return;
            }

            string dllPath = $"{GetLibraryFolder()}{definition.Provider}_L{definition.AccessLevel}.dll";
            Console.WriteLine(dllPath);
            if (!File.Exists(dllPath))
            {
                Console.WriteLine($"Could not find the target security DLL to verify with. {definition.EcuName} ({definition.Provider})");
                Assert.Fail();
                return;
            }

            Console.WriteLine($"Running QuickTest for {definition.EcuName} ({definition.Provider})");
            DllContext context = new DllContext(dllPath, false);

            Tuple<uint, int, int> match = context.AccessLevels.Find(x => x.Item1 == definition.AccessLevel);
            if (match is null)
            {
                Console.WriteLine($"DLL does not support access level {definition.AccessLevel}");
                Assert.Fail();
                return;
            }

            List<byte[]> TestInput = new List<byte[]>();
            foreach (GeneratedByteType byteType in Enum.GetValues(typeof(GeneratedByteType)))
            {
                TestInput.Add(GenerateBytes(definition.SeedLength, byteType));
            }

            bool matchesAreValid = true;
            foreach (byte[] testRow in TestInput)
            {
                byte[] outKey = new byte[definition.KeyLength];
                if (provider.GenerateKey(testRow, outKey, definition.AccessLevel, definition.Parameters))
                {
                    byte[] dllResult = context.GenerateKeyAuto((uint)definition.AccessLevel, testRow);
                    matchesAreValid &= outKey.SequenceEqual(dllResult);
                    Console.WriteLine($"In: {BitUtility.BytesToHex(testRow)} Out: {BitUtility.BytesToHex(outKey)} DLL: {BitUtility.BytesToHex(dllResult)}");
                }
            }
            string testResult = matchesAreValid ? "Passed" : "Failed";
            Console.WriteLine($"QuickTest {testResult}");
            if (!matchesAreValid) 
            {
                Assert.Fail();
            }
        }

        enum GeneratedByteType
        {
            Zeroes,
            Max,
            Ascending,
            Descending,
            Random,
        }

        static byte[] GenerateBytes(int size, GeneratedByteType byteType)
        {
            byte[] output = new byte[size];
            if (byteType == GeneratedByteType.Zeroes)
            {
                // do nothing
            }
            else if (byteType == GeneratedByteType.Max)
            {
                for (int i = 0; i < size; i++)
                {
                    output[i] = 0xFF;
                }
            }
            else if (byteType == GeneratedByteType.Ascending)
            {
                for (int i = 0; i < size; i++)
                {
                    output[i] = (byte)i;
                }
            }
            else if (byteType == GeneratedByteType.Descending)
            {
                for (int i = 0; i < size; i++)
                {
                    output[i] = (byte)(size - 1 - i);
                }
            }
            else if (byteType == GeneratedByteType.Random)
            {
                Random r = new Random();
                r.NextBytes(output);
            }

            return output;
        }

    }
}