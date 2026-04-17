using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnlockECU;

namespace UnlockECUTests
{
    public class IC204Tests
    {
        private List<Definition> definitions;
        private List<SecurityProvider> providers;
        private SecurityProvider ic204Provider;

        [SetUp]
        public void Setup()
        {
            string dbPath = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "Library",
                "db.json");
            string definitionJson = File.ReadAllText(dbPath);
            definitions = System.Text.Json.JsonSerializer.Deserialize<List<Definition>>(definitionJson);
            providers = SecurityProvider.GetSecurityProviders();
            ic204Provider = providers.Find(x => x.GetProviderName() == "IC204");
        }
        
        [TestCase("2049022600", 13, "AABBCCDD11223344", "78FB61DFBB7AEE66")]
        [TestCase("2049022600", 9,  "0000000000000000", "7FE64EA513E37FD5")]
        [TestCase("2049022600", 9,  "1122334455667788", "621AC552C1968E5D")]
        [TestCase("2049022600", 9,  "FEEDFACE0BAD1DEA", "6DFC86ECD8288A30")]
        [TestCase("2049022903", 13, "C1EBF4F94CA0A7A6", "49D4BE45A0B6DFF3")]
        [TestCase("2049022903", 9,  "5C97A0A552FB0205", "D8F169D68D5D17B6")]
        [TestCase("2049028902", 13, "AABBCCDD11223344", "DF47B87060F6DDBD")]
        [TestCase("2049028902", 9,  "0000000000000000", "9B61CBED072F82E9")]
        [TestCase("2129026108", 1,  "0000000000000000", "D632404B132FE12B")]
        [TestCase("2129026108", 13, "0000000000000000", "162FA93147161D00")]
        [TestCase("2129026108", 13, "28C1050F7B52C7CE", "AC12805D446FDF54")]
        [TestCase("2129026108", 13, "537C818A537C818A", "7FD7DCB083DCFB46")]
        [TestCase("2129026108", 3,  "0000000000000000", "6552C46251393265")]
        [TestCase("2129026108", 9,  "0000000000000000", "B83B5E73528197D3")]
        [TestCase("2129026108", 9,  "05E8242E8011F8FF", "B73353CD123F6837")]
        [TestCase("2129026108", 9,  "212A2F38F98A8BD7", "775588C8850CF244")]
        public void KnownKeyPairs(string swVersion, int accessLevel, string seedHex, string expectedKeyHex)
        {
            string ecuName = $"IC204_{swVersion}";
            Definition def = definitions.Find(d => d.EcuName == ecuName && d.AccessLevel == accessLevel);
            Assert.IsNotNull(def, $"Definition for {ecuName} level {accessLevel} should exist in db.json");

            byte[] seed = BitUtility.BytesFromHex(seedHex);
            byte[] expectedKey = BitUtility.BytesFromHex(expectedKeyHex);
            byte[] outKey = new byte[8];

            bool result = ic204Provider.GenerateKey(seed, outKey, accessLevel, def.Parameters);

            Assert.IsTrue(result, "GenerateKey should return true");
            Assert.AreEqual(expectedKey, outKey,
                $"Key mismatch for SW {swVersion} level {accessLevel}: " +
                $"seed={seedHex}, expected={expectedKeyHex}, got={BitUtility.BytesToHex(outKey).ToUpper()}");
        }
    }
}
