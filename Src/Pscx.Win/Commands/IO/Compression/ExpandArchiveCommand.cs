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
using Pscx.Core;
using Pscx.Core.IO;
using SevenZip;
using System.Linq;

namespace Pscx.Commands.IO.Compression {

    /// <summary>
    /// 
    /// </summary>
    [OutputType(typeof(DirectoryInfo), typeof(FileInfo))]
    [Cmdlet(VerbsData.Expand, PscxWinNouns.Archive, DefaultParameterSetName = ParameterSetPath, SupportsShouldProcess = true)]
    [ProviderConstraint(typeof(FileSystemProvider))]
    public class ExpandArchiveCommand : PscxInputObjectPathCommandBase {
        
        public ExpandArchiveCommand() {
            // SevenZipBase.SetLibraryPath(System.IO.Path.Join(PscxContext.Instance.AppsDir, "7z.dll"));
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

        private void ProcessArchive(FileInfo archive) {
            WriteDebug("ProcessArchive: " + archive);
            using (var extractor = new SevenZipExtractor(archive.FullName)) {
                float totalBytes = extractor.ArchiveFileData.Sum(a => (float)a.Size);
                float curExtracted = 0f;

                foreach (ArchiveFileInfo afi in extractor.ArchiveFileData) {
                    extractor.ExtractFiles(OutputPath.ProviderPath, afi.Index);
                    curExtracted += afi.Size;

                    if (ShowProgress) {
                        var progress = new ProgressRecord(1, "Expanding archive...", archive.Name) {
                            RecordType = ProgressRecordType.Processing, CurrentOperation = afi.FileName, PercentComplete = (int)Math.Floor((curExtracted / totalBytes) * 100)
                        };
                        this.WriteProgress(progress);
                        if (this.Stopping) {
                            //cancel entire extraction - should throw PipelineStoppedException instead?
                            WriteWarning("Extraction cancelled.");
                            break;
                        }
                    }

                    if (PassThru) {
                        WriteObject(new ArchiveEntry(afi, archive.FullName, OutputPath.ProviderPath));
                    }
                }

                if (ShowProgress) {
                    var progress = new ProgressRecord(1, "Expanding archive...", archive.Name) {
                        RecordType = ProgressRecordType.Completed, PercentComplete = 100
                    };
                    this.WriteProgress(progress);
                }
            }
        }
    }
}
