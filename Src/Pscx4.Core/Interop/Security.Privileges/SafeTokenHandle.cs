//---------------------------------------------------------------------
// Author: TonyDeSweet
//
// Description: Helper class to implement SafeTokenHandle.
//
// Creation Date: Feb 21, 2006
//---------------------------------------------------------------------
using System;
using System.Security;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using System.Runtime.ConstrainedExecution;
using System.Security.Principal;

namespace Pscx.Interop
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
