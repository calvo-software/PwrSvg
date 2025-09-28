using System;
using System.IO;
using System.Runtime.InteropServices;

namespace PwrSvg
{
    /// <summary>
    /// Module initializer to set up native library search paths for PowerShell cmdlet loading
    /// </summary>
    internal static class NativeLibraryLoader
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetDllDirectory(string lpPathName);
        
        [DllImport("libdl.so.2", EntryPoint = "dlopen")]
        private static extern IntPtr dlopen_linux(string filename, int flags);
        
        [DllImport("libdl.dylib", EntryPoint = "dlopen")]
        private static extern IntPtr dlopen_macos(string filename, int flags);
        
        private static bool _initialized = false;
        private static readonly object _lock = new object();
        
        /// <summary>
        /// Initialize native library search paths for the current platform
        /// </summary>
        public static void Initialize()
        {
            if (_initialized) return;
            
            lock (_lock)
            {
                if (_initialized) return;
                
                try
                {
                    var moduleDir = Path.GetDirectoryName(typeof(NativeLibraryLoader).Assembly.Location);
                    if (string.IsNullOrEmpty(moduleDir)) return;
                    
                    var runtimesDir = Path.Combine(moduleDir, "runtimes");
                    if (!Directory.Exists(runtimesDir)) return;
                    
                    SetupNativeLibraryPath(runtimesDir);
                    _initialized = true;
                }
                catch
                {
                    // Ignore errors during initialization to prevent breaking the module load
                }
            }
        }
        
        private static void SetupNativeLibraryPath(string runtimesDir)
        {
            var architecture = RuntimeInformation.ProcessArchitecture.ToString().ToLowerInvariant();
            string nativeDir = null;
            
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                nativeDir = Path.Combine(runtimesDir, $"win-{architecture}", "native");
                if (Directory.Exists(nativeDir))
                {
                    SetDllDirectory(nativeDir);
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                // For Linux, we need to set LD_LIBRARY_PATH or use dlopen
                nativeDir = Path.Combine(runtimesDir, $"linux-{architecture}", "native");
                if (!Directory.Exists(nativeDir))
                {
                    nativeDir = Path.Combine(runtimesDir, "linux-x64", "native"); // fallback
                }
                
                if (Directory.Exists(nativeDir))
                {
                    // Add to LD_LIBRARY_PATH
                    var currentPath = Environment.GetEnvironmentVariable("LD_LIBRARY_PATH");
                    var newPath = string.IsNullOrEmpty(currentPath) ? nativeDir : $"{nativeDir}:{currentPath}";
                    Environment.SetEnvironmentVariable("LD_LIBRARY_PATH", newPath);
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                nativeDir = Path.Combine(runtimesDir, "osx", "native");
                if (Directory.Exists(nativeDir))
                {
                    // Add to DYLD_LIBRARY_PATH
                    var currentPath = Environment.GetEnvironmentVariable("DYLD_LIBRARY_PATH");
                    var newPath = string.IsNullOrEmpty(currentPath) ? nativeDir : $"{nativeDir}:{currentPath}";
                    Environment.SetEnvironmentVariable("DYLD_LIBRARY_PATH", newPath);
                }
            }
        }
    }
}