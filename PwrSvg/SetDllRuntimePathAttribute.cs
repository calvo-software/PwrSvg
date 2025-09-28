using System;

namespace PwrSvg
{
    /// <summary>
    /// Assembly attribute to automatically initialize native library search paths when the assembly is loaded
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly)]
    public sealed class SetDllRuntimePathAttribute : Attribute
    {
        /// <summary>
        /// Static constructor that automatically initializes native library paths when the assembly loads
        /// </summary>
        static SetDllRuntimePathAttribute()
        {
            NativeLibraryLoader.Initialize();
        }

        /// <summary>
        /// Constructor for the attribute
        /// </summary>
        public SetDllRuntimePathAttribute()
        {
            // Trigger the static constructor if it hasn't been called yet
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(SetDllRuntimePathAttribute).TypeHandle);
        }
    }
}