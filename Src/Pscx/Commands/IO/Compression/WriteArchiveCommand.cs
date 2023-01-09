//---------------------------------------------------------------------
// Authors: Dan Luca
//
// Description: Archive Creation using SharpCompress library
//
// Creation Date: Jan, 2023
//---------------------------------------------------------------------
using System;

using System.Management.Automation;
using SharpCompress.Common;
using SharpCompress.Writers;
using System.Collections.Generic;
using System.IO;
using Microsoft.PowerShell.Commands;
using Pscx.Core.IO;
using System.Linq;

namespace Pscx.Commands.IO.Compression {
    /// <summary>
    /// Class for archive writing
    /// </summary>
    [OutputType(typeof(FileInfo))]
    [Cmdlet(VerbsCommunications.Write, PscxNouns.Archive, DefaultParameterSetName = ParameterSetPath, SupportsShouldProcess = true)]
    [ProviderConstraint(typeof(FileSystemProvider))]
    public class WriteArchiveCommand : PscxInputObjectPathCommandBase {
        /// <summary>
        /// Default archive type and compression
        /// </summary>
        protected ArchiveType archiveType = ArchiveType.Zip;
        protected CompressionType compressionType = CompressionType.Deflate;

        [Parameter(Position = 1, ParameterSetName = ParameterSetLiteralPath)]
        [Parameter(Position = 1, ParameterSetName = ParameterSetPath)]
        [Parameter(Position = 0, ParameterSetName = ParameterSetObject)]
        [PscxPath(NoGlobbing = true)]
        [ValidateNotNullOrEmpty]
        public PscxPathInfo OutputPath { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Parameter(Position = 1, Mandatory = false, ParameterSetName = ParameterSetObject)]
        [PscxPath(NoGlobbing = true, ShouldExist = true, PathType = PscxPathType.Container)]
        [ValidateNotNullOrEmpty, Alias("Root")]
        public PscxPathInfo EntryPathRoot { get; set; }

        [Parameter]
        public SwitchParameter ShowProgress { get; set; }

        [Parameter]
        public SwitchParameter Force { get; set; }

        private List<ArchiveItem> files = new();

        protected static readonly Dictionary<ArchiveType, Dictionary<CompressionType, List<string>>> arTypeExtensions = new() {
            {ArchiveType.Tar, new() { {CompressionType.GZip , new () {".tar", ".tgz"}}}},
            {ArchiveType.GZip, new() { { CompressionType.GZip, new() { ".gzip", ".gz" } } }},
            //{ArchiveType.SevenZip, new() { { CompressionType.GZip, new() { ".7z", ".7zip" } } } },    // not supported yet
            {ArchiveType.Zip, new() {
                { CompressionType.Deflate, new() { ".zip" } },
                { CompressionType.BZip2, new() { ".bz2", "bzip2"} },
                { CompressionType.LZMA, new() { ".lzm", ".lz" } },
                { CompressionType.PPMd, new() { ".pzip", ".pz" } }
            } }
        };

        protected override void BeginProcessing() {
            base.BeginProcessing();

            if (PscxPathInfo.Exists(OutputPath) && Force != true) {
                throw new PipelineStoppedException($"{OutputPath.ProviderPath} already exists and force flag not specified to override. Aborting.");
            }

            string ext = System.IO.Path.GetExtension(OutputPath.ProviderPath);
            if (!string.IsNullOrEmpty(ext)) {
                foreach (KeyValuePair<ArchiveType, Dictionary<CompressionType, List<string>>> cfg in arTypeExtensions) {
                    CompressionType? cpt = cfg.Value.FirstOrDefault(c => c.Value.Contains(ext)).Key;
                    if (cpt.HasValue) {
                        compressionType = cpt.Value;
                        archiveType = cfg.Key;
                    }
                }
            }
        }

        protected override void ProcessPath(PscxPathInfo pscxPath) {
            if (!ShouldProcess(pscxPath.ProviderPath)) {
                return;
            }

            PscxPathType pathType = PscxPathType.None;
            bool pathExists = PscxPathInfo.Exists(pscxPath, ref pathType);
            if (pathExists) {
                switch (pathType) {
                    case PscxPathType.Leaf:
                        files.Add(new ArchiveItem {entryName = System.IO.Path.GetFileName(pscxPath.ProviderPath), file = new FileInfo(pscxPath.ProviderPath)});
                        break;
                    case PscxPathType.Container:
                        foreach (string file in Directory.EnumerateFiles(pscxPath.ProviderPath, "*", SearchOption.AllDirectories)) {
                            files.Add(new ArchiveItem { entryName = file.Substring(pscxPath.ProviderPath.Length), file = new FileInfo(file)});
                        }
                        break;
                    default:
                        WriteWarning($"Unsupported path type {pathType} for location {pscxPath.ProviderPath}");
                        break;
                }
            }
        }

        protected override void EndProcessing() {
            using (Stream stream = File.OpenWrite(OutputPath.ProviderPath))
            using (var writer = WriterFactory.Open(stream, archiveType, new WriterOptions(compressionType) { LeaveStreamOpen = true })) {
                long totalSize = files.Sum(f => f.file.Length);
                long curSize = 0;
                foreach (ArchiveItem ai in files) {
                    writer.Write(ai.entryName, ai.file);
                    if (ShowProgress) {
                        var progress = new ProgressRecord(0, "Creating archive...", OutputPath.ProviderPath) {
                            RecordType = ProgressRecordType.Processing, CurrentOperation = ai.entryName, PercentComplete = (int)Math.Floor(((float)curSize / totalSize) * 100)
                        };
                        this.WriteProgress(progress);
                        curSize += ai.file.Length;
                    }
                }
            }

            if (ShowProgress) {
                var progress = new ProgressRecord(0, "Creating archive...", OutputPath.ProviderPath) {
                    RecordType = ProgressRecordType.Completed, PercentComplete = 100
                };
                this.WriteProgress(progress);
            }

        }

        internal class ArchiveItem {
            internal string entryName;
            internal FileInfo file;
        }
    }
}
