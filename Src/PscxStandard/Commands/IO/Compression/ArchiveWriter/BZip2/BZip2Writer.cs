//---------------------------------------------------------------------
// Authors: Oisin Grehan
//
// Description: Class for writing GZip files.
//
// Creation Date: Jan 4, 2007 
//---------------------------------------------------------------------

using System.IO;

using ICSharpCode.SharpZipLib.BZip2;

namespace Pscx.Commands.IO.Compression.ArchiveWriter.BZip2
{
    /// <summary>
    /// A GZip writer - creates one gz per input file.
    /// </summary>
    class BZip2Writer : MultipleArchiveWriter<WriteBZip2Command, BZip2OutputStream>
    {
        public BZip2Writer(WriteBZip2Command command)
            : base(command)
        {
        }

        public override string DefaultExtension
        {
            get { return ".bz2"; }
        }

        protected override BZip2OutputStream OpenCompressedOutputStream(Stream outputStream)
        {
            Command.WriteVerbose("Opening BZip2 stream.");
            BZip2OutputStream bzip2OutputStream = new BZip2OutputStream(outputStream, WriteArchiveCommandBase.BufferSize);
		    
            this.CurrentOutputStream = bzip2OutputStream;
			
            return bzip2OutputStream;
        }

        protected override void FinishCompressedOutputStream(BZip2OutputStream outputStream)
        {
            if (outputStream != null)
            {
                outputStream.Close();

                this.CurrentOutputStream = null;
                this.OutputCompleted = true;
            }
        }
    }
}