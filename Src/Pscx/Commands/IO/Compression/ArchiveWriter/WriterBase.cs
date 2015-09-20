//---------------------------------------------------------------------
// Authors: jachymko, Oisin Grehan
//
// Description: Base class for writing archive files.
//
// Creation Date: Jan 4, 2007
//---------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.IO;
using Wintellect.PowerCollections;

using Pscx.IO;

namespace Pscx.Commands.IO.Compression.ArchiveWriter
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TCommand"></typeparam>
    /// <typeparam name="TStream"></typeparam>
    abstract class WriterBase<TCommand, TStream> : IArchiveWriter
        where TCommand : WriteArchiveCommandBase
        where TStream : Stream
    {
        private bool _outputCompleted;
        private TStream _currentOutputStream;

        private readonly TCommand _command;
        private readonly Set<string> _excludedPaths;

        // although events are not strictly needed here for simple progress reporting,
        // I like the idea of extensibility for custom events to occur, like emitting 
        // ASCII 7 BEL upon archive completion for notification of a long running task.
        internal event EventHandler<WriteEventArgs> WriteProgress = delegate { };
        internal event EventHandler WriteComplete = delegate { };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        protected WriterBase(TCommand command)
        {
            _command = command;
            _excludedPaths = new Set<string>();

            WriteProgress += command.OnWriteProgress;
        }

        /// <summary>
        /// Releases all resources used by the WriterBase.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged resources and performs other
        /// cleanup operations before the WriterBase is reclaimed 
        /// by garbage collection.
        /// </summary>
        ~WriterBase()
        {
            Dispose(false);
        }

        public abstract string DefaultExtension
        {
            get;
        }

        /// <summary>
        /// Set to true if archive was written succesfully. This is checked in StopProcessing (e.g. ctrl+c)
        /// in determine whether or not to delete the incomplete and most like invalid output archive.
        /// </summary>
        protected bool OutputCompleted
        {
            get { return _outputCompleted; }
            set { _outputCompleted = value; }
        }

        protected TStream CurrentOutputStream
        {
            get { return _currentOutputStream; }
            set { _currentOutputStream = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        protected abstract TStream OpenCompressedOutputStream(Stream stream);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="outputStream"></param>
        /// <param name="path"></param>		
        protected virtual void OpenEntry(TStream outputStream, string path)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="outputStream"></param>
        protected virtual void CloseEntry(TStream outputStream)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="outputStream"></param>
        protected virtual void FinishCompressedOutputStream(TStream outputStream)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytesTotal"></param>
        /// <param name="bytesRead"></param>
        protected virtual void OnWriteProgress(long bytesTotal, long bytesRead)
        {
            WriteEventArgs args = new WriteEventArgs(
                CurrentInputFile, CurrentOutputFile,
                bytesRead, bytesTotal);

            WriteProgress(this, args);
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual void OnWriteComplete()
        {
            Debug.WriteLine("OnWriteComplete", "WriterBase");
            WriteComplete(this, EventArgs.Empty);
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void BeginProcessing()
        {
            Debug.WriteLine("BeginProcessing", "WriterBase");
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void EndProcessing()
        {
            Debug.WriteLine("EndProcessing", "WriterBase");
        }

        /// <summary>
        /// Try to clean up any partially finished archive files, if applicable.
        /// <remarks>We cannot use Command methods here as they will not return due to the cmdlet's "stopping" state.</remarks>
        /// </summary>
        public virtual void StopProcessing()
        {
            Debug.WriteLine("Begin", "WriterBase.StopProcessing");

            if (_outputCompleted == false)
            {
                Debug.WriteLine("Trying to delete partial output...", "WriterBase.StopProcessing");
                try
                {
                    if (_currentOutputStream != null)
                    {
                        // using inner try/catch here as sometimes SharpZipLib
                        // will manage to close the handle, but throw an exception for some
                        // other non-determinate reason, due to invalid state.
                        try
                        {
                            // need to close the file handle as it's locking the output file
                            _currentOutputStream.Close();
                            Debug.WriteLine("Forcibly closed output stream.", "WriterBase.StopProcessing");
                        }
                        catch (Exception ex)
                        {
                            Trace.WriteLine(ex, "WriterBase.StopProcessing Stream Close Error");
                        }
                        File.Delete(CurrentOutputFile);
                        Debug.WriteLine("Deleted partial output.", "WriterBase.StopProcessing");
                    }
                    else
                    {
                        Debug.WriteLine("CurrentOutputStream is null!", "WriterBase.StopProcessing");
                    }
                }
                catch (IOException ex)
                {
                    // ok, we tried our best to delete the output file and still failed.
                    Trace.WriteLine(ex, "WriterBase.StopProcessing Error");
                }
            }
            else
            {
                Debug.WriteLine("CurrentOutputStream appears properly completed before breaking, no need to clean up.",
                                "WriterBase.StopProcessing");
            }
            
            Debug.WriteLine("End", "WriterBase.StopProcessing");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="directory"></param>
        public virtual void ProcessDirectory(DirectoryInfo directory)
        {
            if (Command.ParameterSetName != PscxInputObjectPathCommandBase.ParameterSetObject)
            {
                foreach (string path in Directory.GetFileSystemEntries(directory.FullName))
                {
                    ProcessPath(PscxPathInfo.GetPscxPathInfo(Command.SessionState, path));
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        public virtual void ProcessFile(FileInfo file)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pscxPath"></param>
        public virtual void ProcessPath(PscxPathInfo pscxPath)
        {
            
            DirectoryInfo dir = new DirectoryInfo(pscxPath.ProviderPath);
            FileInfo file = new FileInfo(pscxPath.ProviderPath);

            if (dir.Exists)
            {
                ProcessDirectory(dir);
            }

            if (file.Exists)
            {
                ProcessFile(file);
            }
        }

        protected DateTime GetLastWriteTime(string path)
        {
            FileSystemInfo info;
            
            if (EndsWithDirectorySeparator(path))
            {
                info = new DirectoryInfo(path);
            }
            else
            {
                info = new FileInfo(path);
            }
            return info.LastWriteTime;
        }

        protected static bool EndsWithDirectorySeparator(string path)
        {
            return (path.EndsWith(Path.DirectorySeparatorChar.ToString()) ||
                path.EndsWith(Path.AltDirectorySeparatorChar.ToString()));
        }

        protected bool ShouldClobber(string path)
        {
            if (Command.NoClobber.IsPresent)
            {
                FileInfo file = new FileInfo(path);
                Command.WriteWarning(String.Format(Properties.Resources.ArchiveOutputAlreadyExists, file.Name));
                return false;
            }
            return true;
        }

        /// <summary>
        /// Releases the unmanaged resources used by the
        /// WriterBase and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">
        /// true to release both managed and unmanaged resources; 
        /// false to release only unmanaged resources. 
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <param name="output"></param>
        protected void WriteStream(Stream input, Stream output)
        {
            Command.WriteVerbose("WriteStream input -> output");

            // FIXME: do we risk heap fragmentation on heavy usage?
            //  perhaps a static buffer would be a better choice?
            byte[] buffer = new byte[WriteArchiveCommandBase.BufferSize];
            Copy(input, output, buffer);
        }

        /// <summary>
        /// Copy the contents of one <see cref="Stream"/> to another.
        /// </summary>
        /// <param name="source">The stream to source data from.</param>
        /// <param name="destination">The stream to write data to.</param>
        /// <param name="buffer">The buffer to use during copying.</param>
        /// <remarks>This is taken from SharpZipLib.Core.StreamUtils and modified for progress callbacks.</remarks>
        protected void Copy(Stream source, Stream destination, byte[] buffer)
        {
            PscxArgumentException.ThrowIfIsNull(source, "source");
            PscxArgumentException.ThrowIfIsNull(destination, "destination");
            PscxArgumentException.ThrowIfIsNull(buffer, "buffer");
            PscxArgumentException.ThrowIf(buffer.Length < 128, "Buffer is too small");

            long sourceLength = source.Length;
            long bytesWritten = 0;

            bool copying = true;

            Debug.WriteLine("StartCopy", "WriterBase.Copy");
            while (copying)
            {
                int bytesRead = source.Read(buffer, 0, buffer.Length);
                if (bytesRead > 0)
                {
                    destination.Write(buffer, 0, bytesRead);
                    bytesWritten += bytesRead;

                    OnWriteProgress(sourceLength, bytesWritten);
                }
                else
                {
                    destination.Flush();
                    copying = false;
                }
            }
            OnWriteComplete();
            Debug.WriteLine("EndCopy", "WriterBase.Copy");
        }

        /// <summary>
        /// 
        /// </summary>
        protected abstract string CurrentInputFile
        {
            get;
        }

        /// <summary>
        /// 
        /// </summary>
        protected abstract string CurrentOutputFile
        {
            get;
        }

        /// <summary>
        /// Should this item be excluded from the current output archive?
        /// </summary>
        /// <param name="fullPath">The full path to the item to be tested for exclusion.</param>
        /// <returns>True if the item should be excluded, false if not.</returns>
        /// <remarks>
        /// Typical items that should be excluded are ones that have already been added to the current archive, or items that are a result of the current pipeline (e.g. an archive itself).
        /// </remarks>
        protected virtual bool IsExcludedPath(string fullPath)
        {
            if (_excludedPaths.Contains(fullPath))
            {
                Command.WriteVerbose("Excluded; skipping " + fullPath);
                return true;
            }
            Command.WriteVerbose("New input: " + fullPath);

            return false;
        }

        /// <summary>
        /// Mark an item to be excluded from future processing.
        /// </summary>
        /// <param name="fullPath"></param>
        protected virtual void ExcludePath(string fullPath)
        {
            Command.WriteVerbose("Excluding " + fullPath);
            _excludedPaths.Add(fullPath);
        }

        /// <summary>
        /// 
        /// </summary>
        protected TCommand Command
        {
            get { return _command; }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    internal class WriteEventArgs : EventArgs
    {
        public int Percent
        {
            get
            {
                float percent = ((float)BytesRead / TotalBytes) * 100;
                return (int)percent;
            }
        }

        public readonly long BytesRead;
        public readonly long TotalBytes;
        public readonly string InputFile;
        public readonly string OutputFile;

        public WriteEventArgs(string inFile, string outFile, long bytesRead, long inputLength)
        {
            InputFile = inFile;
            OutputFile = outFile;
            BytesRead = bytesRead;
            TotalBytes = inputLength;
        }
    }
}
