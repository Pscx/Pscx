using System;
using System.IO;
using SevenZip;

namespace Pscx.Commands.IO.Compression.ArchiveReader
{
    internal class PscxSevenZipReader : SevenZipBaseEx
    {
        internal PscxSevenZipReader(PscxCmdlet command, FileInfo file, InArchiveFormat format) :
            base(command, file, format)
        {
            command.WriteDebug(String.Format("Created {0} reader for {1}.", format, file));
        }
    }
}
