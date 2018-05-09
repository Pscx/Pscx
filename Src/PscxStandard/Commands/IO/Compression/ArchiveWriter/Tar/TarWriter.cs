//---------------------------------------------------------------------
// Authors: Oisin Grehan, jachymko
//
// Description: Zip writer which writes all files into a single zip archive.
//
// Creation Date: Jan 2, 2007
//---------------------------------------------------------------------

using System.IO;

using ICSharpCode.SharpZipLib.Tar;

namespace Pscx.Commands.IO.Compression.ArchiveWriter.Tar
{
    /// <summary>
    /// 
    /// </summary>
    class TarWriter : SingleArchiveWriter<WriteTarCommand, TarOutputStream>
    {
        private FileInfo _currentInputFileInfo;

        public TarWriter(WriteTarCommand command)
            : base(command)
        {
			
        }

        public override string DefaultExtension
        {
            get { return ".tar"; }
        }

        public override void ProcessFile(FileInfo file)
        {
            _currentInputFileInfo = file;
            base.ProcessFile(file);
        }

        protected override TarOutputStream OpenCompressedOutputStream(Stream outputStream)
        {
            Command.WriteVerbose("Opening Tar Stream.");
            TarOutputStream tarOutputStream = new TarOutputStream(outputStream);

            this.CurrentOutputStream = tarOutputStream;

            return tarOutputStream;
        }

        protected override void OpenEntry(TarOutputStream outputStream, string path)
        {
            Command.WriteVerbose("Open Tar Entry: " + path);

            TarEntry entry = TarEntry.CreateTarEntry(path);	

            if (EndsWithDirectorySeparator(path))
            {				
                Command.WriteVerbose("Directory Entry.");

                entry.TarHeader.Mode = 511; // 0777
                entry.TarHeader.TypeFlag = TarHeader.LF_DIR;
                entry.TarHeader.Size = 0;
            }
            else
            {
                Command.WriteVerbose("File Entry.");

                entry.TarHeader.Mode = 438; // 0666
                entry.TarHeader.TypeFlag = TarHeader.LF_NORMAL;
                entry.TarHeader.Size = _currentInputFileInfo.Length;
                entry.TarHeader.ModTime = _currentInputFileInfo.LastWriteTime;
            }
			
            outputStream.PutNextEntry(entry);
        }

        protected override void CloseEntry(TarOutputStream outputStream)
        {
            Command.WriteVerbose("Close Tar Entry.");
            outputStream.CloseEntry();
        }

        protected override void FinishCompressedOutputStream(TarOutputStream outputStream)
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