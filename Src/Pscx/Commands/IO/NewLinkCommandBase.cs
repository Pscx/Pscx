//---------------------------------------------------------------------
// Authors: jachymko
//
// Description: Base class for all filesystem link commands.
//
// Creation Date: Dec 13, 2006
//
//---------------------------------------------------------------------
using System;
using System.Management.Automation;
using Microsoft.PowerShell.Commands;
using Pscx.IO;

namespace Pscx.Commands.IO
{
    [ProviderConstraint(typeof(FileSystemProvider))]
    public abstract class NewLinkCommandBase : PscxCmdlet
    {
        [Alias("Path")]
        [Parameter(Position = 0,
                   Mandatory = true,
                   ValueFromPipelineByPropertyName = true,
                   HelpMessage = "Path to the new link.")]
        [PscxPath(NoGlobbing = true, ShouldExist = false)]
        public virtual PscxPathInfo LiteralPath
        {
            set;
            get;
        }

        [Alias("Target", "PSPath")]
        [Parameter(Position = 1,
                   Mandatory = true,
                   ValueFromPipelineByPropertyName = true,
                   HelpMessage = "Target of the link.")]
        [ValidateNotNullOrEmpty]
        [PscxPath(NoGlobbing = true, ShouldExist = true)]
        public virtual PscxPathInfo TargetPath
        {
            get;
            set;
        }
    }
}
