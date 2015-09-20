using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management.Automation;
using Microsoft.PowerShell.Commands;
using Pscx.Commands.IO.Compression.ArchiveReader;
using Pscx.IO;
using SevenZip;

namespace Pscx.Commands.IO.Compression
{
    /// <summary>
    /// Enumerates archives and writes an object for each distinct entry in the archive.
    /// </summary>
    [OutputType(typeof(ArchiveEntry))]
    [Cmdlet(VerbsCommunications.Read, PscxNouns.Archive,
        DefaultParameterSetName = ParameterSetPath)]
    [ProviderConstraint(typeof(FileSystemProvider))]
    public class ReadArchiveCommand : PscxInputObjectPathCommandBase
    {
        public ReadArchiveCommand()
        {
            //this.Format = null;
        }

        //[Parameter(Position = 0, ParameterSetName = ParameterSetObject)]
        //[Parameter(Position = 1, ParameterSetName = ParameterSetPath)]
        //public InArchiveFormat? Format { get; set; }

        /// <summary>
        /// Show progress for reading archives.
        /// </summary>
        [Parameter]
        public SwitchParameter ShowProgress { get; set; }

        /// <summary>
        /// If present, write out an ArchiveEntry object for each directory/folder entry in the archive.
        /// </summary>
        [Parameter]
        public SwitchParameter IncludeDirectories { get; set; }

        protected override bool OnValidatePscxPath(string parameterName, IPscxPathSettings settings)
        {
            if (parameterName == "LiteralPath")
            {
                settings.ShouldExist = true;
            }
            return true;
        }

        protected override void BeginProcessing()
        {
            base.BeginProcessing();

            RegisterInputType<FileInfo>(ProcessArchive);
            RegisterPathInputType<FileInfo>();

            string sevenZDll = PscxContext.Instance.Is64BitProcess ? "7z64.dll" : "7z.dll";
            string sevenZPath = System.IO.Path.Combine(PscxContext.Instance.Home, sevenZDll);;
            
            if (SevenZipBase.CurrentLibraryFeatures == LibraryFeature.None)
            {                
                Trace.Assert(File.Exists(sevenZPath), sevenZPath + " not found or inaccessible.");
                SevenZipBase.SetLibraryPath(sevenZPath); // can only call this once per appdomain
            }

            WriteDebug("7zip path: " + sevenZPath);
            WriteDebug("7zip features: " + SevenZipBase.CurrentLibraryFeatures.ToString());
        }

        protected virtual void ProcessArchive(FileInfo archive)
        {
            var entries = new List<ArchiveEntry>();
            
            // don't need password to dump header/entries
            using (var extractor = new SevenZipExtractor(archive.FullName))
            {
                int total = extractor.ArchiveFileData.Count;
                ProgressRecord progress = null;
                foreach (var info in extractor.ArchiveFileData)
                {
                    if (ShowProgress)
                    {
                        progress = new ProgressRecord(1, "Scanning...", archive.FullName)
                                           {
                                               CurrentOperation = info.FileName,
                                               PercentComplete = (int) Math.Floor(((float) info.Index/total)*100),
                                               RecordType = ProgressRecordType.Processing
                                           };
                        WriteProgress(progress);
                    }

                    if (this.IncludeDirectories || !info.IsDirectory)
                    {
                        entries.Add(new ArchiveEntry(info, archive.FullName, extractor.Format));
                    }
                }
                if (ShowProgress && progress != null)
                {
                    progress.PercentComplete = 100;
                    progress.RecordType = ProgressRecordType.Completed;
                    WriteProgress(progress);
                }
            }

            WriteObject(entries, enumerateCollection:true);
        }

        protected override void ProcessPath(PscxPathInfo pscxPath)
        {
            var archive = new FileInfo(pscxPath.ProviderPath);
            ProcessArchive(archive);
        }
    }
}
