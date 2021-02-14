using Mono.Options;
using System;
using System.Collections.Generic;
using System.IO;
using UnlockECU;

namespace ConsoleUnlockECU
{
    class Program
    {
        static void Main(string[] args)
        {
            bool showHelp = false;
            bool showDefinitions = false;

            int accessLevel = 1;
            string databasePath = "db.json";
            string seedText = "";
            string ecuName = "";
            string prefix = "";

            OptionSet options = new OptionSet()
            {
                {
                    "d|database=", "Path to the JSON definition database",
                    (string v) => databasePath = v 
                },
                {
                    "s|seed=", "Seed value as received from ECU",
                    (string v) => seedText = v
                },
                {
                    "n|name=", "Target ECU name",
                    (string v) => ecuName = v
                },
                {
                    "l|level=", "Access level",
                    (int v) => accessLevel = v
                },
                {
                    "h|help",  "Show this message and exit",
                    v => showHelp = v != null
                },
                {
                    "e|list",  "Show a list of ECU definitions, then exit",
                    v => showDefinitions = v != null
                },
                {
                    "p|prefix=",  "Prefix this string on successful key generation",
                    (string v) => prefix = v
                },
            };

            List<string> extraArgs;
            try
            {
                extraArgs = options.Parse(args);
            }
            catch (OptionException ex)
            {
                Console.WriteLine($"UnlockECU exception: {ex.Message}");
                Console.WriteLine("Try `consoleunlockecu --help' for more information.");
                return;
            }

            if (showHelp)
            {
                ShowHelp(options);
                return;
            }

            // check params if they are valid first
            if (!File.Exists(databasePath))
            {
                Console.WriteLine($"Could not open definition database file. Please check if the database exists at \"{databasePath}\"");
                return;
            }

            string definitionJson = "";
            try
            {
                definitionJson = File.ReadAllText(databasePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UnlockECU exception while reading database : {ex.Message}");
                return;
            }

            List<Definition> definitions = new List<Definition>();
            try
            {
                definitions = System.Text.Json.JsonSerializer.Deserialize<List<Definition>>(definitionJson);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UnlockECU exception while parsing database : {ex.Message}. Please check if the JSON database file is valid.");
                return;
            }

            if (showDefinitions) 
            {
                Console.WriteLine($"UnlockECU: {definitions.Count} definitions available in {databasePath}");
                foreach (Definition d in definitions) 
                {
                    Console.Write($"{d.EcuName} ({d.Origin}): Level {d.AccessLevel}, Seed Length: {d.SeedLength}, Key Length: {d.KeyLength}, Provider: {d.Provider}\n");
                }
                return;
            }

            if (ecuName.Length == 0)
            {
                Console.WriteLine($"UnlockECU: ECU name cannot be empty.");
                ShowHelp(options);
                return;
            }

            Definition matchingDefinition = Definition.FindDefinition(definitions, ecuName, accessLevel);

            if (matchingDefinition is null)
            {
                Console.WriteLine($"UnlockECU exception: Could not find a definition that matches {ecuName} (level {accessLevel})");
                return;
            }

            // clean up user's seed input
            bool validHex = true;
            string cleanedText = seedText.Replace(" ", "").Replace("\r", "").Replace("\n", "").Replace("\t", "").Replace("-", "").ToUpper();
            if (cleanedText.Length % 2 != 0)
            {
                validHex = false;
            }
            if (!System.Text.RegularExpressions.Regex.IsMatch(cleanedText, @"\A\b[0-9a-fA-F]+\b\Z"))
            {
                validHex = false;
            }

            byte[] seed = new byte[] { };
            if (validHex)
            {
                seed = BitUtility.BytesFromHex(cleanedText);
            }
            else if (matchingDefinition.SeedLength == 0) 
            {
                // do nothing, array is already empty
            }
            else
            {
                Console.WriteLine($"UnlockECU exception: ECU {matchingDefinition.EcuName} requires a valid {matchingDefinition.SeedLength}-byte seed");
                return;
            }


            // attempt to generate the key
            SecurityProvider provider = SecurityProvider.GetSecurityProviders().Find(x => x.GetProviderName() == matchingDefinition.Provider);

            if (provider is null)
            {
                Console.WriteLine($"UnlockECU exception: Could not load security provider for {matchingDefinition.EcuName} ({matchingDefinition.Provider})");
                return;
            }
            byte[] outKey = new byte[matchingDefinition.KeyLength];

            if (provider.GenerateKey(seed, outKey, matchingDefinition.AccessLevel, matchingDefinition.Parameters))
            {
                Console.Write($"{prefix}{BitUtility.BytesToHex(outKey)}");
                return;
            }
            else
            {
                Console.WriteLine($"UnlockECU exception: Key generation was unsuccessful for {matchingDefinition.EcuName} ({matchingDefinition.Provider})");
                return;
            }

        }
        
        static void ShowHelp(OptionSet p)
        {
            Console.WriteLine("Usage: consoleunlockecu [OPTIONS]");
            Console.WriteLine("Generates a key for a ECU seed-key challenge");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine();
            p.WriteOptionDescriptions(Console.Out);
        }
    }
}
