using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

using Pscx.Interop;
using Pscx.Interop.SevenZip;

namespace Pscx.Commands.IO.Compression
{
    internal class SevenZipLibrary : IDisposable
    {
        private SafeLibraryHandle _libHandle;

        internal SevenZipLibrary(string sevenZipLibPath)
        {
            if (!File.Exists(sevenZipLibPath))
            {
                throw new FileNotFoundException("Required file not found.", sevenZipLibPath);
            }

            _libHandle = NativeMethods.LoadLibrary(sevenZipLibPath);

            if (_libHandle.IsInvalid)
            {
                throw new Win32Exception("LoadLibrary call for 7z.dll failed: invalid handle!");
            }

            IntPtr functionPtr = NativeMethods.GetProcAddress(_libHandle, "GetHandlerProperty");
            if (functionPtr == IntPtr.Zero)
            {
                _libHandle.Close();
                throw new ArgumentException("7z library: invalid DLL!");
            }
        }

        internal IInArchive CreateInArchive(Guid classId)
        {
            if (_libHandle == null)
            {
                throw new ObjectDisposedException("SevenZipLibary");
            }
                
            IntPtr pointer = CreateInArchiveAsPointer(classId);

            if (pointer != IntPtr.Zero)
            {
                return (IInArchive) Marshal.GetTypedObjectForIUnknown(pointer, typeof (IInArchive));
            }
            return null;
        }

        internal IntPtr CreateInArchiveAsPointer(Guid classId)
        {
            var createObject =
                (CreateObjectDelegate) Marshal.GetDelegateForFunctionPointer(
                                           NativeMethods.GetProcAddress(_libHandle, "CreateObject"),
                                           typeof (CreateObjectDelegate));

            IntPtr pointer = IntPtr.Zero;

            if (createObject != null)
            {
                Guid interfaceId = typeof (IInArchive).GUID;
                createObject(ref classId, ref interfaceId, out pointer);
            }
            
            return pointer;
        }

        private static Dictionary<ArchiveFormat, Guid> s_FFormatClassMap;

        private static Dictionary<ArchiveFormat, Guid> FormatClassMap
        {
            get
            {
                if (s_FFormatClassMap == null)
                {
                    s_FFormatClassMap = new Dictionary<ArchiveFormat, Guid>
                    {
                      {ArchiveFormat.SevenZip, new Guid("23170f69-40c1-278a-1000-000110070000")},
                      {ArchiveFormat.Arj, new Guid("23170f69-40c1-278a-1000-000110040000")},
                      {ArchiveFormat.BZip2, new Guid("23170f69-40c1-278a-1000-000110020000")},
                      {ArchiveFormat.Cab, new Guid("23170f69-40c1-278a-1000-000110080000")},
                      {ArchiveFormat.Chm, new Guid("23170f69-40c1-278a-1000-000110e90000")},
                      {ArchiveFormat.Compound, new Guid("23170f69-40c1-278a-1000-000110e50000")},
                      {ArchiveFormat.Cpio, new Guid("23170f69-40c1-278a-1000-000110ed0000")},
                      {ArchiveFormat.Deb, new Guid("23170f69-40c1-278a-1000-000110ec0000")},
                      {ArchiveFormat.GZip, new Guid("23170f69-40c1-278a-1000-000110ef0000")},
                      {ArchiveFormat.Iso, new Guid("23170f69-40c1-278a-1000-000110e70000")},
                      {ArchiveFormat.Lzh, new Guid("23170f69-40c1-278a-1000-000110060000")},
                      {ArchiveFormat.Lzma, new Guid("23170f69-40c1-278a-1000-0001100a0000")},
                      {ArchiveFormat.Nsis, new Guid("23170f69-40c1-278a-1000-000110090000")},
                      {ArchiveFormat.Rar, new Guid("23170f69-40c1-278a-1000-000110030000")},
                      {ArchiveFormat.Rpm, new Guid("23170f69-40c1-278a-1000-000110eb0000")},
                      {ArchiveFormat.Split, new Guid("23170f69-40c1-278a-1000-000110ea0000")},
                      {ArchiveFormat.Tar, new Guid("23170f69-40c1-278a-1000-000110ee0000")},
                      {ArchiveFormat.Wim, new Guid("23170f69-40c1-278a-1000-000110e60000")},
                      {ArchiveFormat.Z, new Guid("23170f69-40c1-278a-1000-000110050000")},
                      {ArchiveFormat.Zip, new Guid("23170f69-40c1-278a-1000-000110010000")}
                    };
                }
                return s_FFormatClassMap;
            }
        }

        internal static Guid GetClassIdFromKnownFormat(ArchiveFormat format)
        {
            Guid result;
            if (FormatClassMap.TryGetValue(format, out result))
            {
                return result;
            }
            return Guid.Empty;
        }

        ~SevenZipLibrary()
        {
            Dispose(false);
        }

        protected void Dispose(bool disposing)
        {
            _libHandle.Close();
            _libHandle = null;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}