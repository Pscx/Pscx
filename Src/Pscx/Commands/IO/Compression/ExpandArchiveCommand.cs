//---------------------------------------------------------------------
// Authors: Oisin Grehan
//
// Description: Archive Expansion
//
// Creation Date: Jan 7, 2007
//---------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Runtime.InteropServices;
using System.Security;
using Microsoft.PowerShell.Commands;
using Pscx.Commands.IO.Compression.ArchiveReader;
using Pscx.IO;
using SevenZip;

namespace Pscx.Commands.IO.Compression {

    /// <summary>
    /// 
    /// </summary>
    [OutputType(typeof(DirectoryInfo), typeof(FileInfo))]
    [Cmdlet(VerbsData.Expand, PscxNouns.Archive,
        DefaultParameterSetName = ParameterSetPath,
        SupportsShouldProcess = true)]
    [ProviderConstraint(typeof(FileSystemProvider))]
    public class ExpandArchiveCommand : PscxInputObjectPathCommandBase
    {
        private readonly Dictionary<string, SevenZipExtractor> _extractors;
 
        public ExpandArchiveCommand()
        {
            _extractors = new Dictionary<string, SevenZipExtractor>(StringComparer.OrdinalIgnoreCase);
        }

        [Parameter]
        [ValidateNotNull]
        [ValidateRange(0u, uint.MaxValue)]
        public int[] Index { get; set; }

        [Parameter]
        [ValidateNotNullOrEmpty]
        [AcceptsWildcards(true)]
        public string[] EntryPath { get; set; }

        [Parameter(ParameterSetName = ParameterSetObject,      Position = 0, Mandatory = false),
         Parameter(ParameterSetName = ParameterSetPath,        Position = 1, Mandatory = false),
         Parameter(ParameterSetName = ParameterSetLiteralPath, Position = 1, Mandatory = false),
         Alias("To"),
         ValidateNotNullOrEmpty,
         PscxPath(NoGlobbing = true, ShouldExist = true, PathType = PscxPathType.Container)]
        public PscxPathInfo OutputPath { get; set; }

        [Parameter]
        public SecureString Password { get; set; }

        [Parameter]
        public SwitchParameter FlattenPaths { get; set; }

        [Parameter]
        public SwitchParameter PassThru { get; set; }

        [Parameter]
        public SwitchParameter ShowProgress { get; set; }

        [Parameter]
        public SwitchParameter Force { get; set; }

        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            
            RegisterInputType<ArchiveEntry>(ProcessArchive);
            RegisterInputType<FileInfo>(ProcessArchive);
            
            // if OutputPath is not provided, use current filesystem path.
            if (OutputPath == null)
            {
                OutputPath = PscxPathInfo.FromPathInfo(this.CurrentProviderLocation(FileSystemProvider.ProviderName));

                // todo: localize
                WriteVerbose("Using FileSystemProvider current location for OutputPath: " + OutputPath);
            }
            string sevenZDll = PscxContext.Instance.Is64BitProcess ? "7z64.dll" : "7z.dll";
            string sevenZPath = System.IO.Path.Combine(PscxContext.Instance.Home, sevenZDll);

            if (SevenZipBase.CurrentLibraryFeatures == LibraryFeature.None)
            {
                Trace.Assert(File.Exists(sevenZPath), sevenZPath + " not found or inaccessible.");
                SevenZipBase.SetLibraryPath(sevenZPath); // can only call this once per appdomain
            }

            WriteDebug("7zip path: " + sevenZPath);
            WriteDebug("7zip features: " + SevenZipBase.CurrentLibraryFeatures);
        }

        protected override void ProcessPath(PscxPathInfo pscxPath)
        {
            var archive = new FileInfo(pscxPath.ProviderPath);
            ProcessArchive(archive);
        }

        private void ProcessArchive(FileInfo fileInfo)
        {
            WriteDebug("ProcessArchive: " + fileInfo);
            
            //EnsureArchiveFormat(fileInfo);

            string plaintext = null;
            bool passwordSpecified = false;

            if (Password != null)
            {
                passwordSpecified = true;
                IntPtr bstr = Marshal.SecureStringToBSTR(Password);
                plaintext = Marshal.PtrToStringBSTR(bstr);
                Marshal.FreeBSTR(bstr);                
            }

            // we don't need to cache extractor here as we only expect one archive per
            // call to this method.
            using (var extractor = passwordSpecified ?
                new SevenZipExtractor(fileInfo.FullName, password: plaintext) :
                new SevenZipExtractor(fileInfo.FullName))
            {                              
                extractor.PreserveDirectoryStructure = !FlattenPaths;

                WireUpEvents(fileInfo, extractor);

                extractor.FileExtractionFinished += FileExtractionFinishedHandler;

                if (Index == null && EntryPath == null)
                {
                    WriteVerbose("Expanding all.");

                    if (ShouldProcess(fileInfo.FullName, "Expand-Archive"))
                    {
                        extractor.ExtractArchive(OutputPath.ProviderPath);
                    }
                }
                else
                {
                    // we allow mix of -Index and -EntryPath entries
                    if (this.Index != null)
                    {
                        WriteVerbose("Extracting index referenced file(s).");
                        if (ShouldProcess(fileInfo.FullName + " Index list subset", "Expand-Archive"))
                        {
                            extractor.ExtractFiles(OutputPath.ProviderPath, this.Index);
                        }
                    }
                    if (this.EntryPath != null)
                    {
                        WriteVerbose("Extracting path referenced file(s).");
                        if (ShouldProcess(fileInfo.FullName + " EntryPath list subset", "Expand-Archive"))
                        {
                            extractor.ExtractFiles(OutputPath.ProviderPath, this.EntryPath);
                        }
                    }
                }
            }
        }

        private void WireUpEvents(FileInfo fileInfo, SevenZipExtractor extractor)
        {
            // We need access to defaultrunspace, so use synchronous eventing
            extractor.EventSynchronization = EventSynchronizationStrategy.AlwaysSynchronous;

            extractor.FileExists += (sender, args) =>
            {
                // TODO: verify behaviour; in v3, add promptforchoice

                //args.Cancel = !this.Force; // abort entire process?
                //args.FileName = null; // skip this file

                if (!Force)
                {
                    WriteWarning(String.Format("File {0} already exists, skipping.", args.FileName));
                    args.FileName = null; // skip
                }
                else
                {
                    WriteVerbose("Overwrote already existing file " + args.FileName);
                }
            };

            if (ShowProgress)
            {
                WriteVerbose("Enabling progress reporting.");

                extractor.Extracting +=
                    (sender, args) =>
                        {
                            var progress = new ProgressRecord(
                                activityId: 1,
                                activity:
                                    "Expanding archive...",
                                statusDescription: fileInfo.FullName)
                                               {
                                                   RecordType = ProgressRecordType.Processing,
                                                   PercentComplete = args.PercentDone
                                               };

                            this.WriteProgress(progress);

                            if (this.Stopping)
                            {
                                // cancel entire extraction
                                args.Cancel = true;
                                WriteWarning("Extraction cancelled.");
                            }
                        };

                extractor.ExtractionFinished +=
                    (sender, args) =>
                        {
                            var progress = new ProgressRecord(
                                activityId: 1,
                                activity: "Expanding archive...",
                                statusDescription: fileInfo.FullName)
                                               {
                                                   PercentComplete = 100,
                                                   RecordType =
                                                       ProgressRecordType.Completed
                                               };

                            this.WriteProgress(progress);
                        };

                extractor.FileExtractionStarted +=
                    (sender, args) =>
                        {
                            // TODO
                        };

                extractor.FileExtractionFinished +=
                    (sender, args) =>
                        {
                            // TODO
                        };
            }
        }

        private void ProcessArchive(ArchiveEntry entry)
        {
            WriteDebug(String.Format("ProcessArchive: {0}#{1}", entry.ArchivePath, entry.Path));

            string archivePath = entry.ArchivePath;

            SevenZipExtractor extractor;

            // cache extractor(s)
            if (!_extractors.ContainsKey(archivePath))
            {
                _extractors.Add(archivePath,
                    (extractor = new SevenZipExtractor(archivePath)));
                
                extractor.PreserveDirectoryStructure = !FlattenPaths;

                WireUpEvents(new FileInfo(archivePath), extractor);                
            }

            extractor = _extractors[archivePath];

            if ((Index != null) || (EntryPath != null))
            {
                // todo: localize
                WriteWarning("Ignoring -Index and/or -EntryPath arguments.");
            }

            if (ShouldProcess(entry.Path + " from " + entry.ArchivePath, "Expand-Archive"))
            {
                var index = (int)entry.Index;
                extractor.ExtractFiles(OutputPath.ProviderPath, new[] { index });

                if (PassThru)
                {
                    WriteObject(new FileInfo(entry.ArchivePath));
                }
            }
        }

        private void FileExtractionFinishedHandler(object sender, FileInfoEventArgs args)
        {
            if (!PassThru) return;

            string relArchivePathName = args.FileInfo.FileName;
            if (this.FlattenPaths)
            {
                string leafName = System.IO.Path.GetFileName(relArchivePathName);
                string destPath = System.IO.Path.Combine(OutputPath.ProviderPath, leafName);
                if (args.FileInfo.IsDirectory)
                {
                    WriteObject(new DirectoryInfo(destPath));
                }
                else
                {
                    WriteObject(new FileInfo(destPath));
                }
            }
            else
            {
                string destPath = System.IO.Path.Combine(OutputPath.ProviderPath, relArchivePathName);
                if (args.FileInfo.IsDirectory)
                {
                    WriteObject(new DirectoryInfo(destPath));
                }
                else
                {
                    WriteObject(new FileInfo(destPath));
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    foreach (var extractor in _extractors.Values)
                    {
                        extractor.Dispose();
                    }
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }
    }
}
