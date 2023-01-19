using System.Collections.Generic;
using System.IO;
using System.Management.Automation;
using Microsoft.PowerShell.Commands;
using Pscx.Core;
using Pscx.Core.IO;
using SevenZip;
using System.ComponentModel;

namespace Pscx.Commands.IO.Compression {
    /// <summary>
    /// Enumerates archives and writes an object for each distinct entry in the archive.
    /// </summary>
    [OutputType(typeof(ArchiveEntry))]
    [Cmdlet(VerbsCommunications.Read, PscxWinNouns.Archive, DefaultParameterSetName = ParameterSetPath),
        Description("List the contents of an archive - all types supported by 7zip")]
    [ProviderConstraint(typeof(FileSystemProvider))]
    public class ReadArchiveCommand : PscxInputObjectPathCommandBase {
        
        public ReadArchiveCommand() {
            // SevenZipBase.SetLibraryPath(System.IO.Path.Join(PscxContext.Instance.AppsDir, "7z.dll"));
        }

        /// <summary>
        /// If present, write out an ArchiveEntry object for each directory/folder entry in the archive.
        /// </summary>
        [Parameter]
        public SwitchParameter IncludeDirectories { get; set; }

        protected override bool OnValidatePscxPath(string parameterName, IPscxPathSettings settings) {
            if (parameterName == "LiteralPath") {
                settings.ShouldExist = true;
            }
            return true;
        }

        protected override void BeginProcessing() {
            base.BeginProcessing();

            RegisterInputType<FileInfo>(ProcessArchive);
            RegisterPathInputType<FileInfo>();
        }

        protected override void ProcessPath(PscxPathInfo pscxPath) {
            var archive = new FileInfo(pscxPath.ProviderPath);
            ProcessArchive(archive);
        }

        protected void ProcessArchive(FileInfo archive) {
            var entries = new List<ArchiveEntry>();

            using (var extractor = new SevenZipExtractor(archive.FullName)) {
                foreach (ArchiveFileInfo afi in extractor.ArchiveFileData) {
                    if (IncludeDirectories || !afi.IsDirectory) {
                        entries.Add(new ArchiveEntry(afi, archive.FullName));
                    }
                }
            }

            WriteObject(entries, enumerateCollection: true);
        }

    }
}
