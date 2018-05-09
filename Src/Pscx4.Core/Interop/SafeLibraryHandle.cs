using System.Runtime.ConstrainedExecution;
using Microsoft.Win32.SafeHandles;

namespace Pscx.Interop
{
    public sealed class SafeLibraryHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        public SafeLibraryHandle() : base(true) { }

        /// <summary>Release library handle.</summary>
        /// <returns>True if the handle was released.</returns>
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        protected override bool ReleaseHandle()
        {
            return NativeMethods.FreeLibrary(handle);
        }
    }
}