using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management.Automation;
using Microsoft.PowerShell.Commands;
using Pscx.Core.IO;
using SharpCompress.Readers;

namespace Pscx.Commands.IO.Compression {
    /// <summary>
    /// Enumerates archives and writes an object for each distinct entry in the archive.
    /// </summary>
    [OutputType(typeof(ArchiveEntry))]
    [Cmdlet(VerbsCommunications.Read, PscxNouns.Archive, DefaultParameterSetName = ParameterSetPath)]
    [ProviderConstraint(typeof(FileSystemProvider))]
    public class ReadArchiveCommand : PscxInputObjectPathCommandBase {
        public ReadArchiveCommand() {
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

        protected virtual void ProcessArchive(FileInfo archive) {
            var entries = new List<ArchiveEntry>();

            using (Stream stream = File.OpenRead(archive.FullName))
            using (var reader = ReaderFactory.Open(stream)) {
                while (reader.MoveToNextEntry()) {
                    if (!reader.Entry.IsDirectory || IncludeDirectories) {
                        entries.Add(new ArchiveEntry() {Name = reader.Entry.Key});
                    }
                }
            }

            WriteObject(entries, enumerateCollection: true);
        }

        protected override void ProcessPath(PscxPathInfo pscxPath) {
            var archive = new FileInfo(pscxPath.ProviderPath);
            ProcessArchive(archive);
        }
    }
}
