//---------------------------------------------------------------------
// Authors: Oisin Grehan
//
// Description: Tar Creation
//
// Creation Date: Dec 29, 2006
//---------------------------------------------------------------------

using System.IO;
using System.Management.Automation;
using Microsoft.PowerShell.Commands;

using Pscx.Commands.IO.Compression.ArchiveWriter;
using Pscx.Commands.IO.Compression.ArchiveWriter.Tar;
using Pscx.IO;

namespace Pscx.Commands.IO.Compression
{
    [OutputType(typeof(FileInfo))]
	[Cmdlet(VerbsCommunications.Write, PscxNouns.Tar, DefaultParameterSetName = ParameterSetPath, SupportsShouldProcess = true)]
    [ProviderConstraint(typeof(FileSystemProvider))]
	public class WriteTarCommand : WriteArchiveCommandBase {
		
        [ValidateNotNullOrEmpty]
        [Parameter(Mandatory = true, Position = 1)]
        [PscxPath(NoGlobbing = true, ShouldExist = false)]
        public new PscxPathInfo OutputPath
        {
            get { return base.OutputPath; }
            set { base.OutputPath = value; }
        }

        protected override IArchiveWriter OnCreateWriter()
        {
            return new TarWriter(this);
        }
	}
}
