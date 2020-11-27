using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace UnlockECUTests
{
    class UnmanagedUtility
    {
        /*
        Symbol enumeration: 
        https://stackoverflow.com/questions/18249566/c-sharp-get-the-list-of-unmanaged-c-dll-exports
        */

        [DllImport("dbghelp.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SymInitialize(IntPtr hProcess, string UserSearchPath, [MarshalAs(UnmanagedType.Bool)] bool fInvadeProcess);

        [DllImport("dbghelp.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SymCleanup(IntPtr hProcess);

        [DllImport("dbghelp.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern ulong SymLoadModuleEx(IntPtr hProcess, IntPtr hFile, string ImageName, string ModuleName, long BaseOfDll, int DllSize, IntPtr Data, int Flags);

        [DllImport("dbghelp.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SymEnumerateSymbols64(IntPtr hProcess, ulong BaseOfDll, SymEnumerateSymbolsProc64 EnumSymbolsCallback, IntPtr UserContext);

        public delegate bool SymEnumerateSymbolsProc64(string SymbolName, ulong SymbolAddress, uint SymbolSize, IntPtr UserContext);

        /*
        DLL invocation:
        https://stackoverflow.com/questions/16518943/dllimport-or-loadlibrary-for-best-performance
        */

        [DllImport("kernel32.dll")]
        public static extern IntPtr LoadLibrary(string dllToLoad);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

        [DllImport("kernel32.dll")]
        public static extern bool FreeLibrary(IntPtr hModule);


        // Required for textbox placeholder string
        public const int EM_SETCUEBANNER = 0x1501;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern Int32 SendMessage(IntPtr hWnd, int msg, int wParam, [MarshalAs(UnmanagedType.LPWStr)] string lParam);


        private static List<string> LibraryExports = new List<string>();

        public static bool SymbolEnumeratedCallback(string name, ulong address, uint size, IntPtr context)
        {
            // Useful for debug:
            // Console.WriteLine(name);
            LibraryExports.Add(name);
            return true;
        }
        private static bool EnumerateDllExports(string modulePath)
        {
            IntPtr hCurrentProcess = Process.GetCurrentProcess().Handle;

            ulong dllBase;

            // Initialize symbol handler with our own process handle 
            if (!SymInitialize(hCurrentProcess, null, false))
            {
                Console.WriteLine("SymInitialize function (dbghelp.h) failed");
                return false;
            }

            // Load dll
            dllBase = SymLoadModuleEx(hCurrentProcess, IntPtr.Zero, modulePath, null, 0, 0, IntPtr.Zero, 0);

            if (dllBase == 0)
            {
                Console.Out.WriteLine($"Failed to load module: {modulePath}");
                SymCleanup(hCurrentProcess);
                return false;
            }

            // Clean up the results list before it gets populated
            LibraryExports.Clear();

            // Enumerate symbols. For every symbol, the callback method SymbolEnumeratedCallback is called.
            if (SymEnumerateSymbols64(hCurrentProcess, dllBase, SymbolEnumeratedCallback, IntPtr.Zero) == false)
            {
                Console.Out.WriteLine($"Failed to enumerate symbols for library {modulePath}");
                return false;
            }

            SymCleanup(hCurrentProcess);
            return true;
        }

        public static List<string> GetExports(string modulePath)
        {
            if (EnumerateDllExports(modulePath))
            {
                return LibraryExports;
            }
            else
            {
                return new List<string>();
            }
        }

        public static void DumpExportsToConsole(string modulePath)
        {
            List<string> exports = UnmanagedUtility.GetExports(modulePath);
            Console.WriteLine($"Retrieving exports for {modulePath}");
            foreach (string s in exports)
            {
                Console.WriteLine($"{modulePath}: {s}");
            }
            Console.WriteLine($"End of {modulePath} exports.");
        }
    }
}
