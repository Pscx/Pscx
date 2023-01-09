//---------------------------------------------------------------------
// Author: TonyDeSweet
//
// Description: Helper class to implement SafeTokenHandle.
//
// Creation Date: Feb 21, 2006
//---------------------------------------------------------------------

using Microsoft.Win32.SafeHandles;
using System;

namespace Pscx.Win.Interop.Security.Privileges
{
    public sealed class SafeTokenHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        public SafeTokenHandle() : this(IntPtr.Zero)
        {
        }

        public SafeTokenHandle(IntPtr handle) : this(handle, true)
        {
        }

        public SafeTokenHandle(IntPtr handle, bool ownsHandle) : base(ownsHandle)
        {
            SetHandle(handle);
        }

        public static SafeTokenHandle InvalidHandle
        {
            get { return invalid; }
        }

        override protected bool ReleaseHandle()
        {
            return NativeMethods.CloseHandle(handle);
        }

        readonly static SafeTokenHandle invalid = new SafeTokenHandle(IntPtr.Zero, false);
    }
}
