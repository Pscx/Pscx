using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

using Pscx.Commands.IO.Compression.ArchiveReader;
using Pscx.IO;

using SevenZip;

namespace Pscx.Commands.IO.Compression
{
    internal interface ISevenZipBase : IEnumerable<ArchiveEntry>
    {
        bool ShowScanProgress { get; set; }
        PscxCmdlet Command { get; }
        uint EntryCount { get; set; }
        string ArchivePath { get; }
        ArchiveFormat Format { get; }
        void WriteScanProgressRecord(uint index, ArchiveEntry entry);
        void SetProgress(uint total, uint current);
        void SetComplete();
    }

    internal abstract class SevenZipBaseEx : ISevenZipBase, IDisposable
    {
        private readonly SevenZip.SevenZipExtractor _extractor;

        private readonly PscxCmdlet _command;
        private FileInfo _file;
        private InArchiveFormat _format;
        private PscxPathInfo _archivePscxPath;

        protected SevenZipBaseEx(PscxCmdlet command, FileInfo file, InArchiveFormat format)
        {
            //Debug.Assert(format != ArchiveFormat.Unknown, "format != ArchiveFormat.Unknown");

            _command = command;
            _file = file;
            _format = format;
            _archivePscxPath = PscxPathInfo.GetPscxPathInfo(_command.SessionState, file.FullName);

            string sevenZDll = PscxContext.Instance.Is64BitProcess ? "7z64.dll" : "7z.dll";
            string sevenZPath = Path.Combine(PscxContext.Instance.Home, sevenZDll);
            Trace.Assert(File.Exists(sevenZPath), sevenZPath + " not found or inaccessible.");

            SevenZipBase.SetLibraryPath(sevenZPath);
            _extractor = new SevenZipExtractor(file.FullName);            
        }

        public IEnumerator<ArchiveEntry> GetEnumerator()
        {
            foreach (var entry in _extractor.ArchiveFileData)
            {
                yield return new ArchiveEntry(entry, _file.FullName, _format);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool ShowScanProgress { get; set; }
        public PscxCmdlet Command { get; private set; }
        public uint EntryCount { get; set; }
        public string ArchivePath { get; private set; }
        public ArchiveFormat Format { get; private set; }

        public void WriteScanProgressRecord(uint index, ArchiveEntry entry)
        {
            throw new NotImplementedException();
        }

        public void SetProgress(uint total, uint current)
        {
            throw new NotImplementedException();
        }

        public void SetComplete()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            _extractor.Dispose();
        }
    }
/*
    internal abstract class SevenZipBase : IDisposable, ISevenZipBase
    {
        private readonly PscxCmdlet _command;
        private readonly FileInfo _file;
        private readonly ArchiveFormat _format;
        private readonly PscxPathInfo _archivePscxPath;
        private readonly SevenZipLibrary _library;

        protected const int ProgressActivityScan = 1;
        protected const int ProgressActivityExtract = 2;
        protected const int ProgressActivityTest = 2;

        protected bool IsDisposed;

        protected SevenZipBase(PscxCmdlet command, FileInfo file, ArchiveFormat format)
        {
            Debug.Assert(format != ArchiveFormat.Unknown, "format != ArchiveFormat.Unknown");

            _command = command;
            _file = file;
            _format = format;
            _archivePscxPath = PscxPathInfo.GetPscxPathInfo(_command.SessionState, file.FullName);

            string sevenZDll = PscxContext.Instance.Is64BitProcess ? "7z64.dll" : "7z.dll";
            string sevenZPath = Path.Combine(PscxContext.Instance.Home, sevenZDll);
            _library = new SevenZipLibrary(sevenZPath);
        }

        public bool ShowScanProgress
        {
            get;
            set;
        }

        protected SevenZipLibrary Library
        {
            get
            {
                EnsureNotDisposed();
                return _library;
            }
        }

        public PscxCmdlet Command
        {
            get { return _command; }
        }

        public uint EntryCount
        {
            get;
            set;
        }

        public string ArchivePath
        {
            get { return _file.FullName; }
        }

        public ArchiveFormat Format
        {
            get { return _format; }
        }

        protected bool TryGetArchiveEntryProperty(uint index, IInArchive archive, ItemPropId propId, ref PropVariant variant)
        {
            _command.WriteDebug(String.Format("Reading property {0} at index {1}", propId, index));
            EnsureNotDisposed();

            archive.GetProperty(index, propId, ref variant);
            if (variant.VarType != VarEnum.VT_EMPTY)
            {
                return true;
            }
            return false;
        }

        protected ArchiveEntry GetArchiveRecord(uint index, IInArchive archive)
        {
            _command.WriteDebug(String.Format("Reading index {0}", index));
            EnsureNotDisposed();

            var variant = new PropVariant();

            // path
            archive.GetProperty(index, ItemPropId.kpidPath, ref variant);
            var path = (string)variant.GetObject();

            // size
            long size = 0;
            archive.GetProperty(index, ItemPropId.kpidSize, ref variant);
            if (variant.VarType != VarEnum.VT_EMPTY)
            {
                size = variant.longValue;
            }

            // packed size
            long compressedSize = 0;
            archive.GetProperty(index, ItemPropId.kpidPackedSize, ref variant);
            if (variant.VarType != VarEnum.VT_EMPTY)
            {
                compressedSize = variant.longValue;
            }

            // last modified date
            DateTime lastModified = DateTime.MinValue;
            archive.GetProperty(index, ItemPropId.kpidLastWriteTime, ref variant);
            if (variant.VarType != VarEnum.VT_EMPTY)
            {
                lastModified = DateTime.FromFileTime(variant.longValue);
            }

            // name
            string name;
            archive.GetProperty(index, ItemPropId.kpidName, ref variant);
            if (variant.VarType != VarEnum.VT_EMPTY)
            {
                name = (string)variant.GetObject();
            }
            else
            {
                // hack: take from path (trim slash from end so this works with both file and dir).
                name = Path.GetFileName(path.TrimEnd(Path.DirectorySeparatorChar));
            }

            // is folder?
            bool isFolder = false;
            archive.GetProperty(index, ItemPropId.kpidIsFolder, ref variant);
            if (variant.VarType != VarEnum.VT_EMPTY)
            {
                Debug.Assert(variant.VarType == VarEnum.VT_BOOL,
                             "kpidIsFolder: variant.VarType == VarEnum.VT_BOOL; actual: " +
                             variant.VarType);
                isFolder = (variant.byteValue != 0);
            }

            // is encrypted?
            bool isEncrypted = false;
            archive.GetProperty(index, ItemPropId.kpidEncrypted, ref variant);
            if (variant.VarType != VarEnum.VT_EMPTY)
            {
                Debug.Assert(variant.VarType == VarEnum.VT_BOOL,
                             "kpidEncrypted: variant.VarType == VarEnum.VT_BOOL; actual: " +
                             variant.VarType);
                isEncrypted = (variant.byteValue != 0);
            }

            var record = new ArchiveEntry(index, path, size, compressedSize, lastModified, name,
                isEncrypted, isFolder, _file.FullName, _format);

            return record;
        }

        #region IEnumerable<ArchiveEntry> Members

        private IEnumerator<ArchiveEntry> GetSevenZipEnumerator()
        {
            IInArchive archive = null;
            try
            {
                archive = _library.CreateInArchive(
                    SevenZipLibrary.GetClassIdFromKnownFormat(_format));

                if (archive == null)
                {
                    // todo: localize
                    var ex = new InvalidDataException("Creating interface IInArchive failed.");
                    var record = new ErrorRecord(ex, "GetClassIdFromKnownFormat",
                        ErrorCategory.InvalidData, _format);

                    // terminating
                    _command.ErrorHandler.HandleError(true, record);
                }

                using (var stream = new InStreamWrapper(File.OpenRead(_archivePscxPath.ProviderPath)))
                {
                    ulong CheckPos = 32 * 1024;

                    int hr = archive.Open(stream, ref CheckPos, null);
                    if (hr != 0)
                    {
                        // todo: localize
                        var ex = new InvalidOperationException(String.Format("Internal error! hresult: {0:X8}", hr));
                        _command.ErrorHandler.HandleFileError(true, _archivePscxPath.ProviderPath, ex);
                    }


                    EntryCount = archive.GetNumberOfItems();

                    // todo: localize
                    _command.WriteVerbose(EntryCount + " item(s) in archive.");

                    for (uint index = 0; index < EntryCount; index++)
                    {
                        ArchiveEntry entry = null;
                        try
                        {
                            entry = GetArchiveRecord(index, archive);

                            if (ShowScanProgress)
                            {
                                WriteScanProgressRecord(index, entry);
                            }
                        }
                        catch (Exception ex)
                        {
                            // todo: localize
                            var readException = new ArgumentException(
                                String.Format("Error reading index {0}", index), ex);
                            _command.WriteError(new ErrorRecord(readException, "ArchiveReadFail", ErrorCategory.ReadError, null));
                        }
                        if (entry != null)
                        {
                            yield return entry;
                        }
                    }
                }
            }
            finally
            {
                FreeArchive(archive);
            }
        }

        /// <summary>
        /// Release RCW - powershell uses MTA threadpool and 7z's RCW cannot be
        /// shared among threads due to implementation flaws (in 7z).
        /// </summary>
        /// <param name="archive"></param>
        protected void FreeArchive(IInArchive archive)
        {
            if (archive != null)
            {
                Command.WriteDebug(String.Format(
                   "Release reference to IInArchive RCW. Remaining: {0}.",
                   Marshal.ReleaseComObject(archive)));
            }
        }

        public void WriteScanProgressRecord(uint index, ArchiveEntry entry)
        {
            // TODO: localize
            var scanProgress = new ProgressRecord(ProgressActivityScan,
                "Scanning " + _archivePscxPath.ProviderPath, entry.Path)
               {
                   // TODO: log resharper 4.0 bug here (says float cast is redundant)
                   PercentComplete = ((int)(((float)index / (float)EntryCount) * 100))
               };
            Command.WriteProgress(scanProgress);
        }

        IEnumerator<ArchiveEntry> IEnumerable<ArchiveEntry>.GetEnumerator()
        {
            EnsureNotDisposed();
            return GetSevenZipEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            EnsureNotDisposed();
            return ((IEnumerable<ArchiveEntry>)this).GetEnumerator();
            //return GetEnumerator(); // TODO: log resharper 4.0 bug
        }

        #endregion

        #region Static Members

        internal static bool TryGetFormat(PscxCmdlet command, FileInfo archive, ref ArchiveFormat format)
        {
            switch (archive.Extension)
            {
                case ".7z":
                case ".7zip":
                    format = ArchiveFormat.SevenZip;
                    break;

                case ".iso":
                    format = ArchiveFormat.Iso;
                    break;

                case ".zip":
                case ".jar":
                case ".xpi":
                    format = ArchiveFormat.Zip;
                    break;

                case ".arj":
                    format = ArchiveFormat.Arj;
                    break;

                case ".bz":
                case ".bz2":
                case ".tbz":
                case ".tbz2": // bzipped tar
                    format = ArchiveFormat.BZip2;
                    break;

                case ".cab":
                    format = ArchiveFormat.Cab;
                    break;

                case ".chi":
                case ".chm":
                case ".chq":
                case ".chw":
                case ".hxi":
                case ".hxq":
                case ".hxr":
                case ".hxs":
                case ".hxw":
                case ".lit":
                    format = ArchiveFormat.Chm;
                    break;

                case ".deb":
                    format = ArchiveFormat.Deb;
                    break;

                case ".rpm":
                    format = ArchiveFormat.Rpm;
                    break;

                case ".gz":
                case ".gzip":
                case ".tgz":
                case ".tpz":
                    format = ArchiveFormat.GZip;
                    break;

                case ".lha":
                case ".lzh":
                    format = ArchiveFormat.Lzh;
                    break;

                case ".lzma":
                case ".lzma86":
                    format = ArchiveFormat.Lzma;
                    break;

                case ".r00":
                case ".rar":
                    format = ArchiveFormat.Rar;
                    break;

                case ".tar":
                    format = ArchiveFormat.Tar;
                    break;

                case ".z":
                    format = ArchiveFormat.Z;
                    break;

                default:
                    return false;
            }
            return true;
        }

        #endregion

        #region IArchiveReader Members

        public virtual void SetProgress(uint total, uint current)
        {
        }

        public virtual void SetComplete()
        {
        }

        #endregion

        #region IDisposable Members

        protected void EnsureNotDisposed()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    if (_library != null)
                    {
                        _library.Dispose();
                    }
                }
                IsDisposed = true;
            }
        }

        public void Dispose()
        {
            try
            {
                Dispose(true);
            }
            finally
            {
                GC.SuppressFinalize(this);
            }
        }

        ~SevenZipBase()
        {
            Dispose(false);
        }

        #endregion
    }
 */ 
}