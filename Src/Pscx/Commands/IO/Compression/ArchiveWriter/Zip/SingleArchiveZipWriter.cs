//---------------------------------------------------------------------
// Authors: Oisin Grehan, jachymko
//
// Description: Zip writer which writes all files into a single zip archive.
//
// Creation Date: Jan 2, 2007
//---------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;

using ICSharpCode.SharpZipLib.Zip;

namespace Pscx.Commands.IO.Compression.ArchiveWriter.Zip
{
    /// <summary>
    /// 
    /// </summary>
    class SingleArchiveZipWriter : SingleArchiveWriter<WriteZipCommand, ZipOutputStream>
    {
        // <filename, path>
        private readonly Dictionary<String, String> _flattenedPaths;
        private FileSystemInfo _currentInputFileSystemInfo;

        public SingleArchiveZipWriter(WriteZipCommand command) : base(command)
        {
            if (command.FlattenPaths.IsPresent)
            {
                _flattenedPaths = new Dictionary<String, String>();
            }
        }

        public override string DefaultExtension
        {
            get { return ".zip"; }
        }

        public override void ProcessFile(FileInfo file)
        {
            _currentInputFileSystemInfo = file;
            base.ProcessFile(file);
        }

        public override void ProcessDirectory(DirectoryInfo directory)
        {
            _currentInputFileSystemInfo = directory;
            base.ProcessDirectory(directory);
        }

        protected override void ExcludePath(string fullPath)
        {
            if (Command.FlattenPaths.IsPresent)
            {
                _flattenedPaths.Add(Path.GetFileName(fullPath), fullPath);
            }
            base.ExcludePath(fullPath);
        }

        protected override bool IsExcludedPath(string fullPath)
        {
            if (Command.FlattenPaths.IsPresent)
            {
                string fileName = Path.GetFileName(fullPath);
                bool isConflicting = _flattenedPaths.ContainsKey(fileName);

                if (isConflicting)
                {
                    Command.WriteWarning(String.Format(Properties.Resources.ArchiveConflictingFile,
                                                       fileName, _flattenedPaths[fileName]));
                }

                return isConflicting;
            }
            return base.IsExcludedPath(fullPath);
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
            string fullPath = path;

            // if not a directory, flatten path if requested.
            if (Command.FlattenPaths.IsPresent && !EndsWithDirectorySeparator(path))
            {
                path = Path.GetFileName(path);
            }

            ZipEntry entry = new ZipEntry(path);
            entry.DateTime = _currentInputFileSystemInfo.LastWriteTime;

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