using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;


namespace UnlockECUTests
{
    class ExportDefinition
    {
        public static string[] KnownExportedFunctions = new string[] { "GetECUName", "GetComment", "GetKeyLength", "GetSeedLength", "GetConfiguredAccessTypes", "GenerateKeyExOpt", "GenerateKeyEx" };

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr GetECUName();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr GetComment();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int GetKeyLength(uint iSecurityLevel);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int GetSeedLength(uint iSecurityLevel);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int GetConfiguredAccessTypes(uint[] iSecurityLevels);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int GenerateKeyExOpt(byte[] ipSeedArray, uint iSeedArraySize, uint iSecurityLevel, byte[] ipVariant, byte[] ipOptions, byte[] iopKeyArray, uint iMaxKeyArraySize, out uint oActualKeyArraySize);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int GenerateKeyEx(byte[] ipSeedArray, uint iSeedArraySize, uint iSecurityLevel, byte[] ipVariant, byte[] iopKeyArray, uint iMaxKeyArraySize, out uint oActualKeyArraySize);

        public enum VKeyGenResultEx
        {
            OK = 0,
            BufferTooSmall = 1,
            SecurityLevelInvalid = 2,
            VariantInvalid = 3,
            UnknownError = 4
        }
    }
}
