//---------------------------------------------------------------------
// Author: Dan Luca
//
// Description: Class to implement some generic windows utilities.
//
// Creation Date: Jan 2023
//---------------------------------------------------------------------

using Pscx.Win.Interop;
using Pscx.Win.Interop.Security.Privileges;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Pscx.Win {
    public static class Utils {
        private const int MaxPath = 260;

        public static string GetShortPathName(string path) {
            PscxArgumentException.ThrowIfIsNull(path);
            FileSystemInfo info = Core.Utils.GetFileOrDirectory(path);

            if (info == null) {
                throw new FileNotFoundException();
            }

            return GetShortPathName(info);
        }

        public static string GetShortPathName(FileSystemInfo info) {
            PscxArgumentException.ThrowIfIsNull(info);

            if (!info.Exists) {
                if (info is DirectoryInfo) {
                    throw new DirectoryNotFoundException();
                }

                throw new FileNotFoundException();
            }

            StringBuilder buffer = new StringBuilder(MaxPath);
            bool result = NativeMethods.GetShortPathName(info.FullName, buffer, (uint)(buffer.Capacity));

            if (!result) {
                throw new Win32Exception();
            }

            return buffer.ToString();
        }

        public static void AdjustTokenPrivileges(SafeTokenHandle hToken, TokenPrivilegeCollection privileges) {
            byte[] buffer = privileges.ToTOKEN_PRIVILEGES();
            if (!NativeMethods.AdjustTokenPrivileges(hToken, false, buffer, buffer.Length, IntPtr.Zero, IntPtr.Zero)) {
                throw PscxException.LastWin32Exception();
            }
        }

        public static bool? GetIsWow64Process() {
            try {
                UIntPtr proc = NativeMethods.GetProcAddress(NativeMethods.GetModuleHandle("Kernel32.dll"), "IsWow64Process");
                if (proc == UIntPtr.Zero) return null;  // I can't find the answer to this query

                bool retval;
                if (!NativeMethods.IsWow64Process(NativeMethods.GetCurrentProcess(), out retval)) {
                    return null;
                }

                return retval;
            } catch (Exception ex) {
                Trace.WriteLine(String.Format("Failed to determine IsWow64Process: {0}", ex));
                return null;
            }
        }


    }
}
