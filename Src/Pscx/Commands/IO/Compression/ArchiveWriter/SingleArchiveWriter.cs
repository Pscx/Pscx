//---------------------------------------------------------------------
// Authors: Oisin Grehan, jachymko
//
// Description: Archive writer which writes all files into a single archive.
//
// Creation Date: Jan 7, 2007
//---------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using Pscx.IO;

namespace Pscx.Commands.IO.Compression.ArchiveWriter
{
	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="TCommand"></typeparam>
	/// <typeparam name="TStream"></typeparam>
	abstract class SingleArchiveWriter<TCommand, TStream> : WriterBase<TCommand, TStream>
		where TCommand : WriteArchiveCommandBase
		where TStream : Stream
	{
		private readonly string _archivePath;
		private string _currentInputFile;

		private TStream _compressedOutputStream;
		private List<String> _currentPath;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="command"></param>
		protected SingleArchiveWriter(TCommand command)
			: base(command)
		{
            // ReSharper disable DoNotCallOverridableMethodsInConstructor
			ValidateOutputPath();
            // ReSharper restore DoNotCallOverridableMethodsInConstructor
		    
            _archivePath = command.OutputPath.ProviderPath;
			_currentInputFile = null;
		}

		protected virtual void ValidateOutputPath()
		{
		}

		public override void BeginProcessing()
		{
			base.BeginProcessing();

			_currentPath = new List<String>();
			
			ExcludePath(_archivePath);

            Stream outputStream = Command.FileHandler.OpenWrite(_archivePath, Command.NoClobber.IsPresent, false, true);
       		_compressedOutputStream = OpenCompressedOutputStream(outputStream);
		}

        public override void ProcessDirectory(DirectoryInfo directory)
        {
            string fullPath;
            
            if (Command.ParameterSetName == "Object")
            {
                // get specified entry path root, default to CWD if null.
                string root = (Command.EntryPathRoot ??
                               PscxPathInfo.GetPscxPathInfo(Command.SessionState, ".")).ProviderPath;

                if (directory.FullName.StartsWith(root))
                {
                    // trim off the start root
                    fullPath = directory.FullName.Remove(0, root.Length)
                        .Replace('\\', Path.AltDirectorySeparatorChar) // flip slashes
                        .TrimStart(Path.AltDirectorySeparatorChar); // remove leading slash
                }
                else
                {
                    // file falls outside of root, so error out
                    Command.ErrorHandler.WriteFileError(
                        directory.FullName,
                        new ArgumentException(
                            String.Format(
                                Properties.Resources.InputFileOutsideOfRoot,
                                directory.Name,
                                root)));
                    return;
                }

                Command.WriteVerbose("ProcessFile: " + fullPath);
            }
            else
            {
                // note: unix style separator used for ziplib (tar, zip directories etc)
                string name = directory.Name + Path.AltDirectorySeparatorChar;

                string fullName = CurrentPath + name;

                OpenEntry(_compressedOutputStream, fullName);

                _currentPath.Add(name);

                base.ProcessDirectory(directory);

                CloseEntry(_compressedOutputStream);

                _currentPath.RemoveAt(_currentPath.Count - 1);
            }
        }

	    public override void ProcessFile(FileInfo file)
		{
			if (file.FullName == _archivePath)
			{
				return;
			}

			_currentInputFile = file.Name;

		    string fullPath;
		    if (Command.ParameterSetName == "Object")
		    {
                // get specified entry path root, default to CWD if null.
		        string root = (Command.EntryPathRoot ??
                    PscxPathInfo.GetPscxPathInfo(Command.SessionState, ".")).ProviderPath;

                if (file.FullName.StartsWith(root))
                {
                    // trim off the start root
                    fullPath = file.FullName.Remove(0, root.Length)
                        .Replace('\\', Path.AltDirectorySeparatorChar) // flip slashes
                        .TrimStart(Path.AltDirectorySeparatorChar); // remove leading slash

                    Command.WriteVerbose("ProcessFile: " + fullPath);
                }
                else
                {
                    // file falls outside of root, so error out
                    Command.ErrorHandler.WriteFileError(
                        file.FullName, 
                        new ArgumentException(
                            String.Format(
                                Properties.Resources.InputFileOutsideOfRoot,
                                file.Name,
                                root)));
                    return;
                }
		    }
		    else
		    {
                // literalpath / path parameterset
		        fullPath = Path.Combine(CurrentPath, _currentInputFile);
		    }

		    if (!IsExcludedPath(file.FullName))
			{
				OpenEntry(_compressedOutputStream, fullPath);

				Command.FileHandler.ProcessRead(file, delegate(Stream inputStream)
                  	{
                  		WriteStream(inputStream, _compressedOutputStream);
                  		ExcludePath(file.FullName);
                  	});

				CloseEntry(_compressedOutputStream);
			}
		}

		public override void EndProcessing()
		{
			base.EndProcessing();

			FinishCompressedOutputStream(_compressedOutputStream);
            Dispose(true);

			Command.WriteObject(new FileInfo(_archivePath));
		}

        /// <summary>
        /// Releases the unmanaged resources used by the
        /// SingleArchiveWriter and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">
        /// true to release both managed and unmanaged resources; 
        /// false to release only unmanaged resources. 
        /// </param>
        protected override void Dispose(bool disposing)
        {
            if(disposing)
            {
                if(_compressedOutputStream != null)
                {
                    _compressedOutputStream.Dispose();
                }
            }

            base.Dispose(disposing);
        }

		private string CurrentPath
		{
			get { return string.Join(string.Empty, _currentPath.ToArray()); }
		}

		protected override string CurrentOutputFile
		{
			get
			{
				return _archivePath;
			}
		}

		protected override string CurrentInputFile
		{
			get
			{
				return _currentInputFile;
			}
		}
	}
}
