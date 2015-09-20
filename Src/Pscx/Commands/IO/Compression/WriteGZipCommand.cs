//---------------------------------------------------------------------
// Authors: Oisin Grehan
//
// Description: GZip Creation
//
// Creation Date: Dec 29, 2006
//---------------------------------------------------------------------

using System.IO;

using System.Management.Automation;
using Microsoft.PowerShell.Commands;

using Pscx.Commands.IO.Compression.ArchiveWriter;
using Pscx.Commands.IO.Compression.ArchiveWriter.GZip;

namespace Pscx.Commands.IO.Compression
{
    [OutputType(typeof(FileInfo))]
	[Cmdlet(VerbsCommunications.Write, PscxNouns.GZip, DefaultParameterSetName = ParameterSetPath, SupportsShouldProcess = true)]
    [ProviderConstraint(typeof(FileSystemProvider))]
	public class WriteGZipCommand : WriteArchiveCommandBase
	{
		private const int DefaultGZipLevel = 5;
		private const string GZipLevelPreference = "PscxGZipLevelPreference";

	    [PreferenceVariable(GZipLevelPreference, DefaultGZipLevel)]
	    [Parameter(Mandatory = false)]
	    [ValidateRange(1, 9)]
	    public int? Level { get; set; }

	    protected override void BeginProcessing()
		{
			IgnoreInputType<DirectoryInfo>();

			base.BeginProcessing();
		}

        protected override IArchiveWriter OnCreateWriter()
        {
            return new GZipWriter(this);
        }

		protected override PscxInputObjectPathSettings InputSettings
		{
			get
			{
				PscxInputObjectPathSettings settings = base.InputSettings;
				settings.ProcessDirectoryInfoAsPath = false;
				//settings.ProcessFileInfoAsPath = false;
				return settings;
			}
		}
	}
}
