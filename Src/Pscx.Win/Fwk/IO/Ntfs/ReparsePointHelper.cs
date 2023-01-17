//---------------------------------------------------------------------
// Authors: jachymko
//
// Description: Class for managing NTFS reparse points.
//
// Creation Date: Dec 15, 2006
//---------------------------------------------------------------------

using Microsoft.Win32.SafeHandles;
using Pscx.Win.Interop;
using Pscx.Win.Interop.Security.Privileges;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Pscx.Win.Fwk.IO.Ntfs {
    public static class ReparsePointHelper {
        public static string MakeParsedPath(string unparsed) {
            PscxArgumentException.ThrowIfIsNullOrEmpty(unparsed);

            if (unparsed[unparsed.Length - 1] == '\\') {
                unparsed = unparsed.Substring(0, unparsed.Length - 1);
            }

            if (unparsed.StartsWith(UnparsedPathPrefix) && unparsed.Contains(":")) {
                return unparsed.Substring(UnparsedPathPrefix.Length);
            }

            return unparsed;
        }

        public static string MakeUnparsedPath(string parsed) {
            parsed = EnsurePathSlash(parsed);

            if (parsed.StartsWith(UnparsedPathPrefix)) {
                return parsed;
            }

            return UnparsedPathPrefix + parsed;
        }

        public static string EnsurePathSlash(string path) {
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException();

            if (path[path.Length - 1] != '\\') {
                path += '\\';
            }

            return path;
        }

        public static unsafe byte[] GetReparsePointData(string path) {
            if (Directory.Exists(path)) {
                path = EnsurePathSlash(path);
            }

            using (SafeHandle hReparsePoint = OpenReparsePoint(path, false)) {
                if (hReparsePoint.IsInvalid) {
                    throw Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error());
                }

                byte[] bytes = new byte[MAX_REPARSE_SIZE];
                uint bytesReturned = 0;

                fixed (void* buffer = bytes) {
                    if (!UnsafeNativeMethods.DeviceIoControl(hReparsePoint, UnsafeNativeMethods.FSCTL_GET_REPARSE_POINT, null, 0, buffer, MAX_REPARSE_SIZE, out bytesReturned, null)) {
                        throw Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error());
                    }

                }

                byte[] trimmed = new byte[bytesReturned];
                Buffer.BlockCopy(bytes, 0, trimmed, 0, (int)(bytesReturned));

                return trimmed;
            }
        }

        public static unsafe ReparsePointInfo GetReparsePoint(string path) {
            byte[] bytes = GetReparsePointData(path);

            if (bytes == null) {
                return null;
            }

            fixed (byte* buffer = bytes) {
                REPARSE_GUID_DATA_BUFFER* reparseInfo = (REPARSE_GUID_DATA_BUFFER*)buffer;

                if (IsMicrosoftReparseTag(reparseInfo->ReparseTag)) {
                    switch (reparseInfo->ReparseTag) {
                        case ReparsePointType.MountPoint: {
                                MOUNTPOINT_REPARSE_DATA_BUFFER* mp = (MOUNTPOINT_REPARSE_DATA_BUFFER*)buffer;

                                byte* mpdata = buffer + MOUNTPOINT_REPARSE_DATA_BUFFER_HEADER_SIZE;

                                string printName = GetStringFromBuffer(new IntPtr(mpdata + mp->PrintNameOffset), mp->PrintNameLength);
                                string substituteName = GetStringFromBuffer(new IntPtr(mpdata + mp->SubstituteNameOffset), mp->SubstituteNameLength);

                                return new LinkReparsePointInfo(ReparsePointType.MountPoint, path, substituteName);
                            }

                        case ReparsePointType.SymbolicLink: {
                                SYMLINK_REPARSE_DATA_BUFFER* symlink = (SYMLINK_REPARSE_DATA_BUFFER*)buffer;

                                IntPtr ptr = new IntPtr(buffer + symlink->PrintNameOffset + SYMLINK_REPARSE_DATA_BUFFER_HEADER_SIZE);
                                string printName = GetStringFromBuffer(ptr, symlink->PrintNameLength);

                                ptr = new IntPtr(buffer + symlink->SubstituteNameOffset + SYMLINK_REPARSE_DATA_BUFFER_HEADER_SIZE);
                                string substituteName = GetStringFromBuffer(ptr, symlink->SubstituteNameLength);

                                return new LinkReparsePointInfo(ReparsePointType.SymbolicLink, path, substituteName);
                            }
                    }
                }

                return new ReparsePointInfo(path, reparseInfo->ReparseTag);
            }
        }

        public static bool IsReparsePoint(string path) {
            FileSystemInfo info = new FileInfo(path);

            if (!info.Exists) {
                info = new DirectoryInfo(path);
            }

            if (info.Exists) {
                return (info.Attributes & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint;
            }

            return false;
        }

        public static unsafe bool CreateJunction(string junction, string target) {
            byte[] bytes = new byte[MAX_REPARSE_SIZE];
            uint bytesReturned;

            target = MakeUnparsedPath(target);
            int targetLength = Encoding.Unicode.GetBytes(
                                    target, 0, target.Length,
                                    bytes, MOUNTPOINT_REPARSE_DATA_BUFFER_HEADER_SIZE);

            using (SafeHandle hReparsePoint = OpenReparsePoint(junction, true)) {
                if (hReparsePoint.IsInvalid) {
                    throw Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error());
                }

                fixed (void* buffer = bytes) {
                    SET_MOUNTPOINT_REPARSE_DATA_BUFFER* mp = (SET_MOUNTPOINT_REPARSE_DATA_BUFFER*)buffer;
                    mp->ReparseTag = (uint)ReparsePointType.MountPoint;

                    mp->ReparseTargetLength = (ushort)(targetLength);
                    mp->ReparseTargetMaximumLength = (ushort)(targetLength + WCHAR_SIZE);
                    mp->ReparseDataLength = (ushort)(targetLength + 12); // dont ask

                    return UnsafeNativeMethods.DeviceIoControl(
                        hReparsePoint,
                        UnsafeNativeMethods.FSCTL_SET_REPARSE_POINT,
                        buffer, (uint)(mp->ReparseDataLength + 8), // dont ask either
                        null, 0,
                        out bytesReturned,
                        null);
                }
            }
        }

        private static SafeFileHandle OpenReparsePoint(string path, bool writable) {
            AdjustToken();

            NativeMethods.FileAccess access = NativeMethods.FileAccess.GenericRead;

            if (writable) {
                access |= NativeMethods.FileAccess.GenericWrite;
            }

            return NativeMethods.CreateFile(
                path,
                access,
                NativeMethods.FileShare.None,
                IntPtr.Zero,
                NativeMethods.CreationDisposition.OpenExisting,
                NativeMethods.FileAttributes.BackupSemantics |
                NativeMethods.FileAttributes.OpenReparsePoint,
                IntPtr.Zero);
        }

        private static void AdjustToken() {
            using (Process process = Process.GetCurrentProcess()) {
                SafeTokenHandle hToken = SafeTokenHandle.InvalidHandle;

                try {
                    if (!NativeMethods.OpenProcessToken(process.Handle, TokenAccessLevels.AdjustPrivileges, ref hToken)) {
                        throw PscxException.LastWin32Exception();
                    }

                    TokenPrivilegeCollection privileges = new TokenPrivilegeCollection();

                    privileges.Enable(TokenPrivilege.Backup);
                    privileges.Enable(TokenPrivilege.Restore);

                    Utils.AdjustTokenPrivileges(hToken, privileges);
                } finally {
                    hToken.Dispose();
                }
            }
        }

        private static unsafe string GetStringFromBuffer(IntPtr ptr, int length) {
            char[] chars = new char[length];
            Marshal.Copy(ptr, chars, 0, length);

            for (int i = 0; i < length; i++) {
                if (chars[i] == '\0') {
                    length = i;
                    break;
                }
            }

            return new string(chars, 0, length);
        }

        private static bool IsMicrosoftReparseTag(ReparsePointType tag) {
            return ((uint)(tag) & 0x80000000) != 0;
        }

        private const string UnparsedPathPrefix = "\\??\\";

        private const int MOUNTPOINT_REPARSE_DATA_BUFFER_HEADER_SIZE = 16;
        private const int SYMLINK_REPARSE_DATA_BUFFER_HEADER_SIZE = 20;
        private const int MAX_REPARSE_SIZE = 0x4000;
        private const int WCHAR_SIZE = 2;
    }
}
