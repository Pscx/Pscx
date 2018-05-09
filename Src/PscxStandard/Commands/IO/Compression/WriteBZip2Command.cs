//---------------------------------------------------------------------
// Authors: Oisin Grehan
//
// Description: BZip2 Creation
//
// Creation Date: Dec 29, 2006
//---------------------------------------------------------------------
using System.IO;

using System.Management.Automation;

using Microsoft.PowerShell.Commands;

using Pscx.Commands.IO.Compression.ArchiveWriter;
using Pscx.Commands.IO.Compression.ArchiveWriter.BZip2;

namespace Pscx.Commands.IO.Compression 
{
    [OutputType(typeof(FileInfo))]
	[Cmdlet(VerbsCommunications.Write, PscxNouns.BZip2, DefaultParameterSetName = ParameterSetPath, SupportsShouldProcess = true)]
    [ProviderConstraint(typeof(FileSystemProvider))]
	public class WriteBZip2Command : WriteArchiveCommandBase 
    {
        protected override IArchiveWriter OnCreateWriter()
        {
            return new BZip2Writer(this);
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

		protected override void BeginProcessing()
		{
			IgnoreInputType<DirectoryInfo>();
			base.BeginProcessing();
		}
	}
}
