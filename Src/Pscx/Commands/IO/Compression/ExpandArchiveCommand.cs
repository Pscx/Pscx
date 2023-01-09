//---------------------------------------------------------------------
// Authors: Oisin Grehan
//
// Description: Archive Expansion
//
// Creation Date: Jan 7, 2007
//---------------------------------------------------------------------
using System;
using System.IO;
using System.Management.Automation;
using Microsoft.PowerShell.Commands;
using Pscx.Core.IO;
using SharpCompress.Common;
using SharpCompress.Readers;

namespace Pscx.Commands.IO.Compression {

    /// <summary>
    /// 
    /// </summary>
    [OutputType(typeof(DirectoryInfo), typeof(FileInfo))]
    [Cmdlet(VerbsData.Expand, PscxNouns.Archive, DefaultParameterSetName = ParameterSetPath, SupportsShouldProcess = true)]
    [ProviderConstraint(typeof(FileSystemProvider))]
    public class ExpandArchiveCommand : PscxInputObjectPathCommandBase {

        public ExpandArchiveCommand() {
        }

        [Parameter(ParameterSetName = ParameterSetObject, Position = 0, Mandatory = false),
         Parameter(ParameterSetName = ParameterSetPath, Position = 1, Mandatory = false),
         Parameter(ParameterSetName = ParameterSetLiteralPath, Position = 1, Mandatory = false),
         Alias("To"),
         ValidateNotNullOrEmpty,
         PscxPath(NoGlobbing = true, ShouldExist = true, PathType = PscxPathType.Container)]
        public PscxPathInfo OutputPath { get; set; }

        [Parameter]
        public SwitchParameter PassThru { get; set; }

        [Parameter]
        public SwitchParameter ShowProgress { get; set; }

        [Parameter]
        public SwitchParameter Force { get; set; }

        protected override void BeginProcessing() {
            base.BeginProcessing();

            RegisterInputType<FileInfo>(ProcessArchive);

            // if OutputPath is not provided, use current filesystem path.
            if (OutputPath == null) {
                OutputPath = PscxPathInfo.FromPathInfo(this.CurrentProviderLocation(FileSystemProvider.ProviderName));

                // todo: localize
                WriteVerbose("Using FileSystemProvider current location for OutputPath: " + OutputPath);
            }
        }

        protected override void ProcessPath(PscxPathInfo pscxPath) {
            var archive = new FileInfo(pscxPath.ProviderPath);
            ProcessArchive(archive);
        }

        protected bool IsVerbose() {
            if (MyInvocation.BoundParameters.ContainsKey("Verbose")) {
                return (bool)MyInvocation.BoundParameters["Verbose"];
            }

            return false;
        }

        private void ProcessArchive(FileInfo fileInfo) {
            WriteDebug("ProcessArchive: " + fileInfo);

            using (Stream stream = File.OpenRead(fileInfo.FullName))
            using (var reader = ReaderFactory.Open(stream)) {
                long arcSize = fileInfo.Length;
                long bytesProcessed = 0;
                var options = new ExtractionOptions() { ExtractFullPath = true, Overwrite = true };
                while (reader.MoveToNextEntry()) {
                    if (reader.Entry.IsEncrypted) {
                        WriteWarning($"{fileInfo.Name} is password protected - this cmdlet doesn't support encrypted/protected {reader.ArchiveType} archives.");
                        reader.Cancel();
                        break;
                    } 
                    if (!reader.Entry.IsDirectory) {
                        if (IsVerbose()) {
                            Console.WriteLine(reader.Entry.Key);
                        }
                        if (ShowProgress) {
                            bytesProcessed += reader.Entry.CompressedSize;
                            var progress = new ProgressRecord(1, "Expanding archive...", fileInfo.FullName) {
                                RecordType = ProgressRecordType.Processing, CurrentOperation = reader.Entry.Key, PercentComplete = (int)Math.Floor(((float)bytesProcessed / arcSize) * 100)
                            };
                            this.WriteProgress(progress);
                            if (this.Stopping) {
                                //cancel entire extraction
                                reader.Cancel();
                                WriteWarning("Extraction cancelled.");
                            }
                        }
                        reader.WriteEntryToDirectory(OutputPath.ProviderPath, options);
                    }
                }

                if (ShowProgress) {
                    var progress = new ProgressRecord(1, "Expanding archive...", fileInfo.FullName) {
                        RecordType = ProgressRecordType.Completed, PercentComplete = 100
                    };
                    this.WriteProgress(progress);
                }

            }
        }
    }
}
