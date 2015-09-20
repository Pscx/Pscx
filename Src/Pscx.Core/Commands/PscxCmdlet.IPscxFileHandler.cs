//---------------------------------------------------------------------
// Author: jachymko
//
// Description: Interface for opening files.
//
// Creation Date: Dec 29, 2006
//---------------------------------------------------------------------
using System;
using System.IO;

using Pscx.IO;

namespace Pscx.Commands
{
    public interface IPscxFileHandler
    {
        Stream OpenWrite(string filePath);
        Stream OpenWrite(string filePath, bool noClobber);
        Stream OpenWrite(string filePath, bool noClobber, bool force);
        Stream OpenWrite(string filePath, bool noClobber, bool force, bool terminateOnError);
        
        void ProcessRead(string filePath, Action<Stream> action);
        void ProcessRead(FileInfo file, Action<Stream> action);
        
        void ProcessText(string filePath, Action<StreamReader> action);
        void ProcessText(string filePath, bool detectEncodingFromByteMarks, Action<StreamReader> action);
        
        void ProcessWrite(string filePath, Action<Stream> action);
        void ProcessWrite(FileInfo file, Action<Stream> action);
    }

    partial class PscxCmdlet : IPscxFileHandler
    {
        Stream IPscxFileHandler.OpenWrite(string filePath)
        {
            return FileHandler.OpenWrite(filePath, false);
        }

        Stream IPscxFileHandler.OpenWrite(string filePath, bool noClobber)
        {
            return FileHandler.OpenWrite(filePath, noClobber, false);
        }

        Stream IPscxFileHandler.OpenWrite(string filePath, bool noClobber, bool force)
        {
            return FileHandler.OpenWrite(filePath, noClobber, force, false);
        }

        Stream IPscxFileHandler.OpenWrite(string filePath, bool noClobber, bool force, bool terminateOnError)
        {
            try
            {
                FileInfo file = new FileInfo(filePath);
                FileMode mode = noClobber ? FileMode.CreateNew : FileMode.Create;

                if (force && !noClobber && file.Exists && file.IsReadOnly)
                {
                    return new ResetReadOnlyOnDisposeStream(file);
                }

                return file.Open(mode, FileAccess.Write);
            }
            catch (IOException exc)
            {
                ErrorHandler.HandleFileAlreadyExistsError(terminateOnError, filePath, exc);
            }
            catch (Exception exc)
            {
                ErrorHandler.HandleFileError(terminateOnError, filePath, exc);
            }

            return null;
        }

        void IPscxFileHandler.ProcessText(string filePath, bool detectEncodingFromByteMarks, Action<StreamReader> action)
        {
            FileHandler.ProcessRead(filePath, delegate(Stream stream)
            {
                using (StreamReader reader = new StreamReader(stream, detectEncodingFromByteMarks))
                {
                    action(reader);
                }
            });
        }

        void IPscxFileHandler.ProcessText(string filePath, Action<StreamReader> action)
        {
            FileHandler.ProcessText(filePath, true, action);
        }

        void IPscxFileHandler.ProcessRead(string filePath, Action<Stream> action)
        {
            Stream stream = null;

            try
            {
                try
                {
                    stream = File.OpenRead(filePath);
                }
                catch (Exception exc)
                {
                    ErrorHandler.WriteFileError(filePath, exc);
                }

                if (stream != null)
                {
                    action(stream);
                }
            }
            finally
            {
                if (stream != null)
                {
                    stream.Dispose();
                }
            }
        }

        void IPscxFileHandler.ProcessRead(FileInfo file, Action<Stream> action)
        {
            FileHandler.ProcessRead(file.FullName, action);
        }

        void IPscxFileHandler.ProcessWrite(string filePath, Action<Stream> action)
        {
            Stream stream = null;

            try
            {
                try
                {
                    stream = File.Create(filePath);
                }
                catch (Exception exc)
                {
                    ErrorHandler.WriteFileError(filePath, exc);
                }

                if (stream != null)
                {
                    action(stream);
                }
            }
            finally
            {
                if (stream != null)
                {
                    stream.Dispose();
                }
            }
        }

        void IPscxFileHandler.ProcessWrite(FileInfo file, Action<Stream> action)
        {
            FileHandler.ProcessWrite(file.FullName, action);
        }

        class ResetReadOnlyOnDisposeStream : StreamDecorator
        {
            private readonly FileInfo _file;
            private bool _disposed;

            public ResetReadOnlyOnDisposeStream(FileInfo file)
            {
                _file = file;
                _file.IsReadOnly = false;

                InnerStream = _file.OpenWrite();
            }

            public override void Close()
            {
                base.Close();
                
                ResetReadOnly();
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);

                if (disposing)
                {
                    ResetReadOnly();
                }
            }

            private void ResetReadOnly()
            {
                if( _disposed)
                {
                    return;
                }
                
                _file.IsReadOnly = true;
                _disposed = true;
            }
        }
    }
}
