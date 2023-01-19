
using SevenZip;
using System;

namespace Pscx.Commands.IO.Compression {
    /// <summary>
    /// Represents an entry in a compressed archive, like a ZIP or TAR file.
    /// This class only contains metadata about the entry.
    /// </summary>
    [Serializable]
    public class ArchiveEntry {
        #nullable enable
        public ArchiveEntry(ArchiveFileInfo archiveFileInfo, string archivePath, string? outPath = null) {
            Index = (uint) archiveFileInfo.Index;
            Path = System.IO.Path.Join(outPath ?? string.Empty, archiveFileInfo.FileName);
            Size = archiveFileInfo.Size;
            Name = System.IO.Path.GetFileName(archiveFileInfo.FileName);
            ModifiedDate = archiveFileInfo.LastWriteTime;            
            IsEncrypted = archiveFileInfo.Encrypted;
            IsFolder = archiveFileInfo.IsDirectory;
            ArchivePath = archivePath;
            CompressionMethod = archiveFileInfo.Method;
            CRC = archiveFileInfo.Crc;
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
        public string CompressionMethod { get; private set; }

        /// <summary>
        /// The full path to the containing archive file, e.g. foo.zip
        /// </summary>
        public string ArchivePath { get; private set; }

        public uint CRC { get; private set; }


    }
}
