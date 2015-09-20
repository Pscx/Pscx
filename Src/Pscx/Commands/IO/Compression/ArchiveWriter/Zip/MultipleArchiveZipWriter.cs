//---------------------------------------------------------------------
// Authors: Oisin Grehan, jachymko
//
// Description: Zip writer which writes each file into it's own zip archive.
//
// Creation Date: Jan 2, 2007
//---------------------------------------------------------------------
using System.IO;

using ICSharpCode.SharpZipLib.Zip;

namespace Pscx.Commands.IO.Compression.ArchiveWriter.Zip
{
    /// <summary>
    /// 
    /// </summary>
    class MultipleArchiveZipWriter : MultipleArchiveWriter<WriteZipCommand, ZipOutputStream>
    {
        public MultipleArchiveZipWriter(WriteZipCommand command)
            : base(command)
        {
        }

        public override string DefaultExtension
        {
            get { return ".zip"; }
        }

        protected override ZipOutputStream OpenCompressedOutputStream(Stream outputStream)
        {
            Command.WriteVerbose("Opening Zip stream.");
            ZipOutputStream zipOutputStream = new ZipOutputStream(outputStream);
            zipOutputStream.SetLevel(Command.Level.Value);

            this.CurrentOutputStream = zipOutputStream;

            return zipOutputStream;
        }

        protected override void OpenEntry(ZipOutputStream outputStream, string path)
        {
            // always flatten the path
            string shortPath = Path.GetFileName(path);

            ZipEntry entry = new ZipEntry(shortPath);
            entry.DateTime = GetLastWriteTime(path);

            outputStream.PutNextEntry(entry);
        }

        protected override void FinishCompressedOutputStream(ZipOutputStream outputStream)
        {
            if(outputStream != null)
            {
                Command.WriteVerbose("Closing Zip stream.");
                outputStream.Finish();

                this.CurrentOutputStream = null;
                this.OutputCompleted = true;
            }
        }
    }
}