using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;

namespace UnlockECUTests
{
    /// <summary>
    /// High level interface to access a Vector DLL
    /// </summary>
    public class DllContext
    {
        private IntPtr dllHandle = IntPtr.Zero;
        public List<string> DllExports = new List<string>();
        private Dictionary<string, IntPtr> dllAddressMappings = new Dictionary<string, IntPtr>();

        public string SHA1Hash = "";
        public string FileDescription = "";
        public string FileName = "";
        public string DLLPath = "";
        public string ECUName = "";
        public string Comment = "";
        public bool KeyGenerationCapability = false;
        public bool ModeSpecified = false;
        public List<Tuple<uint, int, int>> AccessLevels = new List<Tuple<uint, int, int>>();

        public DllContext(string filePath, bool runHash = true)
        {
            DLLPath = filePath;
            if (!File.Exists(DLLPath))
            {
                Console.WriteLine($"{DLLPath}: File does not exist");
                return;
            }
            FileName = Path.GetFileName(filePath);

            // Compute and store the file hash
            if (runHash)
            {
                using (var cryptoProvider = new SHA1CryptoServiceProvider())
                {
                    SHA1Hash = BitConverter.ToString(cryptoProvider.ComputeHash(File.ReadAllBytes(filePath))).Replace("-", "");
                }
            }

            // Get the module's exports
            DllExports = UnmanagedUtility.GetExports(DLLPath);

            if (DllExports.Count == 0)
            {
                Console.WriteLine($"{DLLPath}: No exports, possibly an invalid DLL");
                return;
            }

            // Try to load the library into our process space
            dllHandle = UnmanagedUtility.LoadLibrary(filePath);
            if (dllHandle == IntPtr.Zero)
            {
                Console.WriteLine($"{DLLPath}: LoadLibrary failed");
                return;
            }

            // Try to load addresses of all known exports
            dllAddressMappings = new Dictionary<string, IntPtr>();
            foreach (string knownExport in ExportDefinition.KnownExportedFunctions)
            {
                if (DllExports.Contains(knownExport))
                {
                    dllAddressMappings.Add(knownExport, UnmanagedUtility.GetProcAddress(dllHandle, knownExport));
                }
                else
                {
                    dllAddressMappings.Add(knownExport, IntPtr.Zero);
                }
            }

            // Set capabilities
            KeyGenerationCapability = DllExports.Contains("GenerateKeyEx") || DllExports.Contains("GenerateKeyExOpt");
            ModeSpecified = DllExports.Contains("GetKeyLength") && DllExports.Contains("GetSeedLength") && DllExports.Contains("GetConfiguredAccessTypes");

            // Store additional metadata
            FileDescription = FileVersionInfo.GetVersionInfo(DLLPath).FileDescription;

            LoadAdditionalDataFromDllCalls();
        }

        public void LoadAdditionalDataFromDllCalls()
        {
            ECUName = GetECUName();
            Comment = GetComment();

            if (!ModeSpecified)
            {
                return;
            }

            // Access level, key size, seed size
            AccessLevels = new List<Tuple<uint, int, int>>();
            foreach (uint accessLevel in GetConfiguredAccessTypes())
            {
                AccessLevels.Add(new Tuple<uint, int, int>(accessLevel, GetKeyLength(accessLevel), GetSeedLength(accessLevel)));
            }
        }

        // Automatically selects and invokes the correct key generation function. Prefers the "opt" variant
        public byte[] GenerateKeyAuto(uint securityLevel, byte[] seed)
        {
            if (DllExports.Contains("GenerateKeyExOpt"))
            {
                return GenerateKey(seed, securityLevel, true, out ExportDefinition.VKeyGenResultEx result);
            }
            else if (DllExports.Contains("GenerateKeyEx"))
            {
                return GenerateKey(seed, securityLevel, false, out ExportDefinition.VKeyGenResultEx result);
            }
            else
            {
                return new byte[] { };
            }
        }

        public string GetECUName()
        {
            IntPtr procAddress = dllAddressMappings["GetECUName"];
            if (procAddress == IntPtr.Zero)
            {
                return "(unavailable)";
            }
            var fn = (ExportDefinition.GetECUName)Marshal.GetDelegateForFunctionPointer(procAddress, typeof(ExportDefinition.GetECUName));
            IntPtr resultPtr = fn();
            return Marshal.PtrToStringAnsi(resultPtr);
        }
        public string GetComment()
        {
            IntPtr procAddress = dllAddressMappings["GetComment"];
            if (procAddress == IntPtr.Zero)
            {
                return "(unavailable)";
            }
            var fn = (ExportDefinition.GetComment)Marshal.GetDelegateForFunctionPointer(procAddress, typeof(ExportDefinition.GetComment));
            IntPtr resultPtr = fn();
            return Marshal.PtrToStringAnsi(resultPtr);
        }
        public List<uint> GetConfiguredAccessTypes()
        {
            IntPtr procAddress = dllAddressMappings["GetConfiguredAccessTypes"];
            if (procAddress == IntPtr.Zero)
            {
                return new List<uint>();
            }
            uint[] accessTypes = new uint[1000];
            var fn = (ExportDefinition.GetConfiguredAccessTypes)Marshal.GetDelegateForFunctionPointer(procAddress, typeof(ExportDefinition.GetConfiguredAccessTypes));
            int accessTypesCount = fn(accessTypes);
            return accessTypes.Take(accessTypesCount).ToList();
        }
        public int GetSeedLength(uint securityLevel)
        {
            IntPtr procAddress = dllAddressMappings["GetSeedLength"];
            if (procAddress == IntPtr.Zero)
            {
                return 0;
            }
            var fn = (ExportDefinition.GetSeedLength)Marshal.GetDelegateForFunctionPointer(procAddress, typeof(ExportDefinition.GetSeedLength));
            return fn(securityLevel);
        }
        public int GetKeyLength(uint securityLevel)
        {
            IntPtr procAddress = dllAddressMappings["GetKeyLength"];
            if (procAddress == IntPtr.Zero)
            {
                return 0;
            }
            var fn = (ExportDefinition.GetKeyLength)Marshal.GetDelegateForFunctionPointer(procAddress, typeof(ExportDefinition.GetKeyLength));
            return fn(securityLevel);
        }

        private byte[] GenerateKey(byte[] seed, uint securityLevel, bool addOptionParameter, out ExportDefinition.VKeyGenResultEx returnError)
        {
            returnError = ExportDefinition.VKeyGenResultEx.UnknownError;

            IntPtr procAddress = dllAddressMappings[addOptionParameter ? "GenerateKeyExOpt" : "GenerateKeyEx"];
            if ((!KeyGenerationCapability) || procAddress == IntPtr.Zero)
            {
                return new byte[] { };
            }
            byte[] keyResult = new byte[0x1000];
            uint actualkeySize;
            int keygenResult = (int)returnError;

            if (addOptionParameter)
            {
                var fn = (ExportDefinition.GenerateKeyExOpt)Marshal.GetDelegateForFunctionPointer(procAddress, typeof(ExportDefinition.GenerateKeyExOpt));
                keygenResult = fn(seed, (uint)seed.Length, securityLevel, null, null, keyResult, (uint)keyResult.Length, out actualkeySize);
            }
            else
            {
                var fn = (ExportDefinition.GenerateKeyEx)Marshal.GetDelegateForFunctionPointer(procAddress, typeof(ExportDefinition.GenerateKeyEx));
                keygenResult = fn(seed, (uint)seed.Length, securityLevel, null, keyResult, (uint)keyResult.Length, out actualkeySize);
            }
            returnError = (ExportDefinition.VKeyGenResultEx)keygenResult;

            keyResult = keyResult.Take((int)actualkeySize).ToArray();
            return keyResult;
        }

        public void UnloadLibrary()
        {
            // WARNING: the instance will no longer be able to access native functions after this is called
            // This is a workaround if many DLLs have to be enumerated for their metadata -- Windows has a limit on the number of DLLs that can be loaded simultaneously
            UnmanagedUtility.FreeLibrary(dllHandle);
        }

        ~DllContext()
        {
            if (dllHandle != IntPtr.Zero)
            {
                UnmanagedUtility.FreeLibrary(dllHandle);
            }
        }
    }
}

