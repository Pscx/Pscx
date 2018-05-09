//---------------------------------------------------------------------
// Authors: Oisin Grehan
//
// Description: Archive Writer which writes each file into it's own archive.
//
// Creation Date: Jan 7, 2007
//---------------------------------------------------------------------
using System;
using System.IO;

namespace Pscx.Commands.IO.Compression.ArchiveWriter
{
	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="TCommand"></typeparam>
	/// <typeparam name="TStream"></typeparam>
    abstract class MultipleArchiveWriter<TCommand, TStream> : WriterBase<TCommand, TStream>
		where TCommand : WriteArchiveCommandBase
		where TStream : Stream
    {
		private string _currentArchive;
		private string _currentInputFile;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="command"></param>
		protected MultipleArchiveWriter(TCommand command)
            : base(command) 
		{
			_currentArchive = null;
			_currentInputFile = null;
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="inputPath"></param>
		/// <param name="outputPath"></param>
		protected virtual void WritePath(string inputPath, string outputPath)
		{
			if (inputPath.Equals(outputPath, StringComparison.CurrentCultureIgnoreCase))
			{
				Command.WriteVerbose("Skipping own output: " + inputPath);
				return;
			}

			// save for WriteEventArgs
			_currentArchive = outputPath;
			_currentInputFile = inputPath;

			Command.WriteVerbose("WritePath inputPath -> outputPath");

            // Reset flag for StopProcessing abort/cleanup handling.
            this.OutputCompleted = false;

			// open read stream
			Command.FileHandler.ProcessRead(inputPath,
				delegate(Stream inputStream)
				{
					// open write stream
					Command.FileHandler.ProcessWrite(outputPath,
						delegate(Stream outputStream)
						{
							ExcludePath(outputPath);

							// wrap output stream in compression stream (will Dispose)
							using(TStream compressedStream = OpenCompressedOutputStream(outputStream))
                            {
								if (!IsExcludedPath(inputPath))
								{
									// add entry
									OpenEntry(compressedStream, inputPath);

									// write input to output
									WriteStream(inputStream, compressedStream);

									// do not allow reprocessing of this path
									ExcludePath(inputPath);

									// close entry, if needed
									CloseEntry(compressedStream);
								}

                            	// finish the stream, if needed
                                FinishCompressedOutputStream(compressedStream);
							}

							Command.WriteObject(new FileInfo(outputPath));
						}
					);
				}
			);
		}

        public override void ProcessFile(FileInfo file)
		{
			Command.WriteVerbose("ProcessFile: " + file.Name);

			string outputPath = (file.FullName + DefaultExtension);

			if (File.Exists(outputPath) && (!ShouldClobber(outputPath)))
			{
				return;
			}

			WritePath(file.FullName, outputPath);
		}

		protected override string CurrentInputFile
		{
			get
			{
				return _currentInputFile;
			}
		}

		protected override string CurrentOutputFile
		{
			get
			{
				return _currentArchive;
			}
		}
    }
}
