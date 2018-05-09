using System;

using SevenZip;

namespace Pscx.Commands.IO.Compression.ArchiveReader
{
    /// <summary>
    /// Represents an entry in a compressed archive, like a ZIP or TAR file.
    /// This class only contains metadata about the entry.
    /// </summary>
    [Serializable]
    public class ArchiveEntry
    {
        internal ArchiveEntry(ArchiveFileInfo archiveEntry, string archivePath, InArchiveFormat archiveFormat)
        {
            Index = (uint) archiveEntry.Index;
            Path = archiveEntry.FileName;
            Size = archiveEntry.Size;
            Name = System.IO.Path.GetFileName(archiveEntry.FileName);
            CompressedSize = 0; // Not supported in SevenZipSharp (yet)
            ModifiedDate = archiveEntry.LastWriteTime;            
            IsEncrypted = archiveEntry.Encrypted;
            IsFolder = archiveEntry.IsDirectory;
            ArchivePath = archivePath;
            Format = archiveFormat;
            CRC = archiveEntry.Crc;
        }

        /// <summary>
        /// Index of item in archive directory.
        /// </summary>
        public uint Index { get; private set; }

        /// <summary>
        /// kpidPath
        /// </summary>
        public string Path { get; private set; }

        /// <summary>
        /// kpidSize
        /// </summary>
        public ulong Size { get; private set; }

        /// <summary>
        /// kpidPackedSize (missing from SevenZipSharp)
        /// </summary>
        public long CompressedSize { get; private set; }

        /// <summary>
        /// kpidLastWriteTime
        /// </summary>
        public DateTime ModifiedDate { get; private set; }

        /// <summary>
        /// kpidName
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Does this entry come from an encrypted archive?
        /// </summary>
        public bool IsEncrypted { get; private set; }

        /// <summary>
        /// Does this entry represent a folder in the archive?
        /// </summary>
        public bool IsFolder { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public InArchiveFormat Format { get; private set; }

        /// <summary>
        /// The full path to the containing archive file, e.g. foo.zip
        /// </summary>
        public string ArchivePath { get; private set; }

        public uint CRC { get; private set; }
    }
}
