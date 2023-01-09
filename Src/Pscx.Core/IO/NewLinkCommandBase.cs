//---------------------------------------------------------------------
// Authors: jachymko
//
// Description: Base class for all filesystem link commands.
//
// Creation Date: Dec 13, 2006
//
//---------------------------------------------------------------------

using Microsoft.PowerShell.Commands;
using Pscx.Commands;
using System.Management.Automation;

namespace Pscx.Core.IO {
    [ProviderConstraint(typeof(FileSystemProvider))]
    public abstract class NewLinkCommandBase : PscxCmdlet {
        [Alias("Path")]
        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Path to the new link.")]
        [PscxPath(NoGlobbing = true, ShouldExist = false)]
        public virtual PscxPathInfo LiteralPath {
            set;
            get;
        }

        [Alias("Target", "PSPath")]
        [Parameter(Position = 1, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Target of the link.")]
        [ValidateNotNullOrEmpty]
        [PscxPath(NoGlobbing = true, ShouldExist = true, PathType = PscxPathType.LeafOrContainer)]
        public virtual PscxPathInfo TargetPath {
            get;
            set;
        }
    }
}
