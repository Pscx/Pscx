//---------------------------------------------------------------------
// Author: jachymko
//
// Description: Helper class for PInvoke Win32API.
//
// Creation Date: Dec 14, 2006
//---------------------------------------------------------------------

using Pscx.Win.Interop.Security.Privileges;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;

// I don't care if there are some values that aren't used yet in this interop class.
#pragma warning disable 0414

namespace Pscx.Win.Interop
{
    public static unsafe class UnsafeNativeMethods
    {
        #region Constants
        public const int ERROR_INVALID_REPARSE_DATA = 0x1128;
        public const int ERROR_REPARSE_TAG_INVALID = 0x1129;
        public const int ERROR_REPARSE_TAG_MISMATCH = 0x112a;

        public const int FSCTL_GET_REPARSE_POINT = 0x900a8;
        public const int FSCTL_SET_REPARSE_POINT = 0x900a4;
        public const int FSCTL_DELETE_REPARSE_POINT = 0x900ac;
        #endregion

        #region kernel32!DeviceIoControl

        [DllImport(Dll.Kernel32, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool DeviceIoControl(
                  SafeHandle hDevice,
                  uint dwIoControlCode,
             [In] void* lpInBuffer,
                  uint nInBufferSize,
            [Out] void* lpOutBuffer,
                  uint nOutBufferSize,
            [Out] out uint lpBytesReturned,
             [In] void* lpOverlapped);

        #endregion

        #region advapi32!GetTokenInformation
        
        [DllImport(Dll.Advapi32, CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool GetTokenInformation(
                 SafeTokenHandle TokenHandle,
                 TOKEN_INFORMATION_CLASS TokenInfoClass,
           [Out] void* TokenInformation,
                 int TokenInfoLength,
           [Out] out int ccbReturn);


        #endregion
    }
}
