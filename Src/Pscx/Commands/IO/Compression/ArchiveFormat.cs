using System;

namespace Pscx.Commands.IO.Compression
{
    [Serializable]
    public enum ArchiveFormat
    {
        Unknown = -1,
        SevenZip,
        Arj,
        BZip2,
        Cab,
        Chm,
        Compound,
        Cpio,
        Deb,
        GZip,
        Iso,
        Lzh,
        Lzma,
        Nsis,
        Rar,
        Rpm,
        Split,
        Tar,
        Wim,
        Z,
        Zip,        
    }
}