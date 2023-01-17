//---------------------------------------------------------------------
// Authors: Dan Luca
//
// Description: Archive Creation using SharpCompress library
//
// Creation Date: Jan, 2023
//---------------------------------------------------------------------
using System;
using System.Management.Automation;
using System.Collections.Generic;
using System.IO;
using Microsoft.PowerShell.Commands;
using Pscx.Core;
using Pscx.Core.IO;
using SevenZip;

namespace Pscx.Commands.IO.Compression {
    /// <summary>
    /// Class for archive writing
    /// </summary>
    [OutputType(typeof(FileInfo))]
    [Cmdlet(VerbsCommunications.Write, PscxWinNouns.Archive, DefaultParameterSetName = ParameterSetPath, SupportsShouldProcess = true)]
    [ProviderConstraint(typeof(FileSystemProvider))]
    public class WriteArchiveCommand : PscxInputObjectPathCommandBase {

        public WriteArchiveCommand() {
            SevenZipBase.SetLibraryPath(System.IO.Path.Join(PscxContext.Instance.AppsDir, "7z.dll"));
        }

        /// <summary>
        /// Default archive type and compression
        /// </summary>
        private OutArchiveFormat archiveType = OutArchiveFormat.Zip;
        private CompressionMethod compressionMethod = CompressionMethod.Default;
        private CompressionMode compressionMode = CompressionMode.Create;

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

        private Dictionary<string, string> filesToArchive = new();

        protected override void BeginProcessing() {
            base.BeginProcessing();

            if (PscxPathInfo.Exists(OutputPath) && Force != true) {
                compressionMode = CompressionMode.Append;
            }

            try {
                InArchiveFormat fmt = Formats.FormatByFileName(OutputPath.ProviderPath, true);
                OutArchiveFormat outFormat = OutArchiveFormat.Zip;
                Enum.TryParse(Enum.GetName(typeof(InArchiveFormat), fmt), out outFormat);
                archiveType = outFormat;
            } catch (Exception ex) {
                WriteWarning($"Cannot infer an archive type from extension {System.IO.Path.GetExtension(OutputPath.ProviderPath)} - defaulting to Zip");
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
                        filesToArchive.Add(getEntryName(pscxPath.ProviderPath, pscxPath), pscxPath.ProviderPath);
                        break;
                    case PscxPathType.Container:
                        filesToArchive.Add(getEntryName(pscxPath.ProviderPath, pscxPath), null);    //add the directory entry - the file full path field is null
                        foreach (string file in Directory.EnumerateFiles(pscxPath.ProviderPath, "*", SearchOption.AllDirectories)) {
                            bool isDir = (File.GetAttributes(file) & FileAttributes.Directory) == FileAttributes.Directory;
                            filesToArchive.Add(getEntryName(file, pscxPath), isDir ? null : file);
                        }
                        break;
                    default:
                        WriteWarning($"Unsupported path type {pathType} for location {pscxPath.ProviderPath}");
                        break;
                }
            }
        }

        protected override void EndProcessing() {
            var compressor = new SevenZipCompressor() {
                ArchiveFormat = archiveType, CompressionMethod = compressionMethod, CompressionMode = compressionMode, DirectoryStructure = true, PreserveDirectoryRoot = false
            };
            //long totalSize = filesToArchive.Values.Where(v => v != null).Sum(f => new FileInfo(f).Length);
            if (ShowProgress) {
                compressor.FileCompressionFinished += (sender, args) => {
                    FileNameEventArgs fnea = args as FileNameEventArgs;
                    var progress = new ProgressRecord(0, "Creating archive...", OutputPath.ProviderPath) {
                        RecordType = ProgressRecordType.Processing, CurrentOperation = fnea.FileName, PercentComplete = fnea.PercentDone};
                    this.WriteProgress(progress);
                };
            }

            compressor.CompressFileDictionary(filesToArchive, OutputPath.ProviderPath);

            if (ShowProgress) {
                var progress = new ProgressRecord(0, "Creating archive...", OutputPath.ProviderPath) {
                    RecordType = ProgressRecordType.Completed, PercentComplete = 100
                };
                this.WriteProgress(progress);
            }

        }

        private string getEntryName(string fullFilePath, PscxPathInfo pscxPath) {
            if (EntryPathRoot != null && fullFilePath.Contains(EntryPathRoot.ProviderPath)) {
                return fullFilePath.Substring(fullFilePath.IndexOf(EntryPathRoot.ProviderPath, StringComparison.Ordinal) + EntryPathRoot.ProviderPath.Length);
            }

            if (fullFilePath.Contains(pscxPath.ProviderPath)) {
                return fullFilePath.Substring(fullFilePath.IndexOf(pscxPath.ProviderPath, StringComparison.Ordinal) + pscxPath.ProviderPath.Length);
            }
            return pscxPath.SourcePath;
        }
    }
}
