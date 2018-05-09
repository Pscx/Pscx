using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;


using System.Management.Automation;

using Pscx.Commands.IO.Compression.ArchiveReader;
using Pscx.Interop.SevenZip;
using SevenZip;
using OperationResult = Pscx.Interop.SevenZip.OperationResult;

namespace Pscx.Commands.IO.Compression.ArchiveExtracter
{
    internal class PscxSevenZipExtractor : SevenZipBaseEx
    {
        // need to cache entries here to avoid RCW 
        // marshalling interfaces between threads
        // which can happen when using delegates
        private readonly List<ArchiveEntry> _entries;
        private readonly bool _passThru;

        internal PscxSevenZipExtractor(PscxCmdlet command, FileInfo file, bool passThru, InArchiveFormat format) :
            base(command, file, format)
        {
            _passThru = passThru;
            _entries = new List<ArchiveEntry>();
            foreach (ArchiveEntry entry in this){
                _entries.Add(entry);
            }
        }

        internal bool IgnoreCase { get; set; }

        internal string Password { get; set; }

        internal bool FlattenPaths { get; set; }

        internal bool PassThru { get; set; }

        internal string OutputDirectory { get; set; }

        internal bool ShowExtractProgress { get; set; }
        
        internal string Transform { get; set; }

        internal void Extract(ArchiveEntry wantedEntry)
        {
            Extract(entry => entry.Index == wantedEntry.Index);
        }

        internal void Extract(string entryPath)
        {
            if (WildcardPattern.ContainsWildcardCharacters(entryPath))
            {
                Command.WriteVerbose("Using wildcard extraction.");
                var pattern = new WildcardPattern(entryPath, WildcardOptions.IgnoreCase);
                Extract(entry => pattern.IsMatch(entry.Path));
            }
            else
            {
                // todo: fix ignorecase
                Extract(entry => entry.Path.Equals(entryPath,
                    StringComparison.OrdinalIgnoreCase));
            }
        }

        internal void Extract()
        {
            // extract all
            Extract(entry => true);
        }

        internal void Extract(Predicate<ArchiveEntry> isMatch)        
        {
            var entriesToExtract = new Dictionary<uint, string>();
            var modificationTimes = new Dictionary<uint, DateTime>();
            foreach (ArchiveEntry entry in _entries)
            {
                // hack: skip folders
                if (isMatch(entry) && (!entry.IsFolder))
                {
                    entriesToExtract.Add(entry.Index, entry.Path);
                    modificationTimes.Add(entry.Index, entry.ModifiedDate);
                }
            }

            //IInArchive archive = null;
            //try
            //{
            //    archive = Library.CreateInArchive(
            //        SevenZipLibrary.GetClassIdFromKnownFormat(base.Format));

            //    if (archive == null)
            //    {
            //        throw new InvalidOperationException("Create IInArchive Fail!");
            //    }

            //    using (var stream = new InStreamWrapper(File.OpenRead(base.ArchivePath)))
            //    {
            //        ulong CheckPos = 32 * 1024;
            //        if (archive.Open(stream, ref CheckPos, null) != 0)
            //        {
            //            throw new InvalidOperationException("CheckPos Fail! May not be a valid " + base.Format + " archive.");
            //        }

            //        var indices = new uint[entriesToExtract.Count];
            //        entriesToExtract.Keys.CopyTo(indices, 0);

            //        var callback = new ArchiveExtractCallback(entriesToExtract, modificationTimes, Command, _passThru, OutputDirectory, ShowExtractProgress, Transform);
            //        int hr = archive.Extract(indices, (uint)indices.Length, 0, callback);
            //        if (hr != 0)
            //        {
            //            Command.WriteVerbose("Extract returned " + hr);

            //            if (Enum.IsDefined(typeof(OperationResult), hr))
            //            {
            //                var result = (OperationResult) hr;
            //                Command.WriteWarning(result.ToString());
            //            }
            //            else
            //            {
            //                string error;
            //                switch (hr)
            //                {
            //                    case -2147024784: // 0x80070070
            //                        error = "Insufficient space on output device.";
            //                        break;
            //                    default:
            //                        error = String.Format("Unknown error: {0:x} ({0})", hr);
            //                        break;
            //                }
            //                Command.WriteWarning(error);
            //            }
            //        }
            //    }
            //}
            //finally
            //{
            //    FreeArchive(archive);
            //}

            foreach (ArchiveEntry entry in _entries)
            {
                // hack: create empty folders
                if (isMatch(entry) && (entry.IsFolder))
                {                    
                    if (!Directory.Exists(Path.Combine(OutputDirectory, entry.Path)))
                    {
                        Directory.CreateDirectory(Path.Combine(OutputDirectory, entry.Path));
                    }
                }
            }
        }

        private class ArchiveOpenCallback : IArchiveOpenCallback, IArchiveOpenVolumeCallback,
            ICryptoGetTextPassword
        {
            #region Implementation of IArchiveOpenCallback

            public void SetTotal(IntPtr files, IntPtr bytes)
            {
                throw new System.NotImplementedException();
            }

            public void SetCompleted(IntPtr files, IntPtr bytes)
            {
                throw new System.NotImplementedException();
            }

            #endregion

            #region Implementation of IArchiveOpenVolumeCallback

            public void GetProperty(ItemPropId propID, IntPtr value)
            {
                throw new System.NotImplementedException();
            }

            public int GetStream(string name, out IInStream inStream)
            {
                throw new System.NotImplementedException();
            }

            #endregion

            #region Implementation of ICryptoGetTextPassword

            public int CryptoGetTextPassword(out string password)
            {
                throw new System.NotImplementedException();
            }

            #endregion
        }

        private class ArchiveExtractCallback : IArchiveExtractCallback, ICryptoGetTextPassword
        {
            private readonly Dictionary<uint, string> _entries;
            private readonly Dictionary<uint, DateTime> _modificationTimes;
            private OutStreamWrapper _fileStream;
            private readonly PscxCmdlet _command;
            private readonly string _outputPath;
            private string _currentFileName;
            private string _transform = null;

            private ulong _total;
            private string _currentEntry = null;
            private readonly bool _passThru;
            private readonly bool _showProgress;
            private uint _currentIndex;

            internal ArchiveExtractCallback(Dictionary<uint,string> entries, Dictionary<uint,DateTime> modificationTimes, PscxCmdlet command, bool passThru, string outputPath, bool showProgress, string transform)
            {
                _passThru = passThru;
                _entries = entries;
                _command = command;
                _outputPath = outputPath;
                _showProgress = showProgress;
                _transform = transform;
                _modificationTimes = modificationTimes;
            }

            #region IArchiveExtractCallback Members

            public void SetTotal(ulong total) // IProgress
            {
                Debug.WriteLine("ArchiveCallback->SetTotal: " + total);
                _total = total;
            }

            public void SetCompleted(ref ulong completeValue) // IProgress
            {
                Debug.WriteLine("ArchiveCallback->SetCompleted: " + completeValue);
                try
                {
                    int percentage = ((int)(((float)completeValue / (float)_total) * 100));
                    if (percentage > 100)
                    {
                        percentage = 100;
                    }
                    if ((_currentEntry != null) && (_showProgress ))
                    {
                        var progress = new ProgressRecord(1, "Expanding", _currentEntry)
                           {
                               PercentComplete = percentage
                           };                        
                        _command.WriteProgress(progress);
                    }
                }
                catch (PipelineStoppedException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _command.WriteWarning("ArchiveCallback->SetCompleted error: " + ex.ToString());
                }
            }

            public int GetStream(uint index, out ISequentialOutStream outStream, AskMode askExtractMode)
            {
                //return 0x80004004L; // E_ABORT
                // S_FALSE = 1
                // S_OK = 0

                // todo: check if index matches an entry in _entries, setup the stream.
                Debug.WriteLine(String.Format("Mode: {0}; ArchiveCallback->GetStream {1}", askExtractMode, index));
                _currentIndex = index;
                if (askExtractMode == AskMode.kSkip)
                {
                    var stream = new OutStreamWrapper(new MemoryStream());
                    outStream = stream;
                    return 0;
                }
                string _transform_pattern = null;
                string _transform_text = null;

                // if transform is specified, make sure it is well formed
                if (_transform != null)
                {
                    // substitute pattern specified in _transform
                    // _transform is split by "/", i.e. "regex/replacement"
                    string[] transform_data = _transform.Split('/');
                    if (transform_data.Length == 2)
                    {
                        _transform_pattern = transform_data[0];
                        _transform_text = transform_data[1];
                        // remove any escape chars from the text
                        _transform_text = Regex.Unescape(_transform_text);
                        _command.WriteDebug("ArchiveCallback->GetStream transform_pattern: " + _transform_pattern);
                        _command.WriteDebug("ArchiveCallback->GetStream transform_text: " + _transform_text);
                    }
                    else
                    {
                        // bad transform, error...
                        _command.WriteWarning("Transform must be in the form regex/text!");
                        outStream = null;
                        return 1; // S_FALSE
                    }
                }

                try
                {
                    if ((_entries.ContainsKey(index)) && (askExtractMode == AskMode.kExtract))
                    {
                        // ensure output directory is base
                        _currentEntry = _entries[index];
                        if (_transform_pattern != null)
                        {
                            // substitute pattern specified in _transform_pattern with _transform_text
                            Regex transformer = new Regex(_transform_pattern);
                            // only run through replacement if there is a match for this entry
                            if (transformer.IsMatch(_currentEntry))
                            {
                                string newFilename = transformer.Replace(_currentEntry, _transform_text);
                                _command.WriteVerbose("Transform: " + _currentEntry + " to: " + newFilename);
                                _currentFileName = Path.Combine(_outputPath, newFilename);
                            }
                            else
                            {
                                _currentFileName = Path.Combine(_outputPath, _entries[index]);
                            }
                        }
                        else
                        {
                            _currentFileName = Path.Combine(_outputPath, _entries[index]);
                        }

                        string fileDir = Path.GetDirectoryName(_currentFileName);
                        if (!string.IsNullOrEmpty(fileDir))
                        {
                            Directory.CreateDirectory(fileDir);
                        }
                        _fileStream = new OutStreamWrapper(File.Create(_currentFileName));
                        outStream = _fileStream;
                    }
                    else
                    {
                        outStream = null;
                    }
                }
                catch (PipelineStoppedException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _command.WriteWarning("ArchiveCallBack->GetStream error: " + ex.ToString());
                    outStream = null;

                    return 1; // S_FALSE
                }
                return 0; // S_OK
            }

            public void PrepareOperation(AskMode askExtractMode)
            {
                Debug.WriteLine("ArchiveCallback->PrepareOperation: " + askExtractMode);
            }

            public void SetOperationResult(OperationResult resultEOperationResult)
            {
                Debug.WriteLine("ArchiveCallback->SetOperationResult: " + resultEOperationResult);
                if (_fileStream != null)
                {                    
                    _fileStream.Dispose();

                    // should pass-thru?
                    var fileInfo = new FileInfo(_currentFileName);
                    // set creation time
                    if (fileInfo.Exists) {
                        //_command.WriteVerbose("Setting creation and lastwrite time of " + _currentFileName + " to: " + _modificationTimes[_currentIndex].ToLocalTime());
                        File.SetCreationTime(_currentFileName,_modificationTimes[_currentIndex]);
                        File.SetLastWriteTime(_currentFileName, _modificationTimes[_currentIndex]);
                        File.SetLastAccessTime(_currentFileName, _modificationTimes[_currentIndex]);
                    }
                    if (_passThru && fileInfo.Exists)
                    {
                        _command.WriteObject(fileInfo);
                    }
                }
            }

            #endregion

            #region ICryptoGetTextPassword Members

            public int CryptoGetTextPassword(out string password)
            {
                _command.WriteWarning("Need password!");
                password = null;

                return 1; // S_FALSE
            }

            #endregion
        }
    }
}