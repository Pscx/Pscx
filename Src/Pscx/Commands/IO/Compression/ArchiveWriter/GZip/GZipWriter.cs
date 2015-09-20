//---------------------------------------------------------------------
// Authors: Oisin Grehan
//
// Description: Class for writing GZip files.
//
// Creation Date: Jan 4, 2007
//
// TODO: honour OutputPath - all gz files should go into this folder if specified.
//---------------------------------------------------------------------

using System.IO;

using ICSharpCode.SharpZipLib.GZip;

namespace Pscx.Commands.IO.Compression.ArchiveWriter.GZip
{
    /// <summary>
    /// A GZip writer - creates one gz per input file.
    /// </summary>
    class GZipWriter : MultipleArchiveWriter<WriteGZipCommand, GZipOutputStream>
    {		
        public GZipWriter(WriteGZipCommand command) : base(command)
        {
        }

        public override string DefaultExtension
        {
            get { return ".gz"; }
        }

        protected override GZipOutputStream OpenCompressedOutputStream(Stream outputStream)
        {
            Command.WriteVerbose("Opening GZip stream.");
		    
            GZipOutputStream gzipOutputStream = new GZipOutputStream(outputStream, WriteArchiveCommandBase.BufferSize);
            gzipOutputStream.SetLevel(Command.Level.Value);

            this.CurrentOutputStream = gzipOutputStream;

            return gzipOutputStream;
        }

        protected override void FinishCompressedOutputStream(GZipOutputStream gzipOutputStream)
        {
            if (gzipOutputStream != null)
            {
                Command.WriteVerbose("Closing GZip stream.");
                gzipOutputStream.Finish();

                this.CurrentOutputStream = null;
                this.OutputCompleted = true;
            }
        }
    }
}