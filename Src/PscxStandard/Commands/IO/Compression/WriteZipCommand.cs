//---------------------------------------------------------------------
// Authors: Oisin Grehan, jachymko
//
// Description: Zip Creation
//
// Creation Date: Dec 29, 2006
//---------------------------------------------------------------------

using System.IO;
using System.Management.Automation;
using Microsoft.PowerShell.Commands;

using Pscx.Commands.IO.Compression.ArchiveWriter;
using Pscx.Commands.IO.Compression.ArchiveWriter.Zip;

namespace Pscx.Commands.IO.Compression
{
    [OutputType(typeof(FileInfo))]
    [Cmdlet(VerbsCommunications.Write, PscxNouns.Zip, DefaultParameterSetName = ParameterSetPath, SupportsShouldProcess = true)]
    [ProviderConstraint(typeof(FileSystemProvider))]
    public class WriteZipCommand : WriteArchiveCommandBase
    {
		private const int DefaultLevel = 5;

		private const string LevelPreference = "PscxZipLevelPreference";
		private const string IncludeEmptyDirsPreference = "PscxZipIncludeEmptyDirsPreference";
    	private const string FlattenPathsPreference = "PscxZipFlattenPathsPreference";

        [Parameter(Mandatory = false)]
        [ValidateRange(1, 9)]
        [PreferenceVariable(LevelPreference, DefaultLevel)]
        public int? Level { get; set; }

        [Parameter(Mandatory = false)]
        [PreferenceVariable(IncludeEmptyDirsPreference)]
        public SwitchParameter IncludeEmptyDirectories { get; set; }

        [Parameter(Mandatory = false)]
        [PreferenceVariable(FlattenPathsPreference)]
        public SwitchParameter FlattenPaths { get; set; }

        [Parameter]
        public SwitchParameter Append { get; set; }

        protected override IArchiveWriter OnCreateWriter()
        {
            IArchiveWriter writer;
			
            if (OutputPath == null)
			{
                // todo: localize
				WriteVerbose("Mode: Each file is compressed separately.");				
                writer = new MultipleArchiveZipWriter(this);
                if (FlattenPaths.IsPresent)
                {
                    WriteWarning("In multiple zip mode, the FlattenPaths parameter is redundant: All zip files created contain a single file with no path.");
                }
			}
			else
			{
                if (Append.IsPresent)
                {
                    // todo: localize
                    WriteVerbose("Mode: single archive update of " + OutputPath);
                    writer = new SingleArchiveZipUpdater(this);
                }
                else
                {
                    // todo: localize
                    WriteVerbose("Mode: All files are compressed into " + OutputPath);
                    writer = new SingleArchiveZipWriter(this);
                }
			}
            return writer;
        }
    }
}
