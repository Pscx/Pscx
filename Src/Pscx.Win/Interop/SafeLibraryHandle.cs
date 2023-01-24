using Microsoft.Win32.SafeHandles;
using System.Runtime.ConstrainedExecution;

namespace Pscx.Win.Interop
{
    public sealed class SafeLibraryHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        public SafeLibraryHandle() : base(true) { }

        /// <summary>Release library handle.</summary>
        /// <returns>True if the handle was released.</returns>
        protected override bool ReleaseHandle()
        {
            return NativeMethods.FreeLibrary(handle);
        }
    }
}