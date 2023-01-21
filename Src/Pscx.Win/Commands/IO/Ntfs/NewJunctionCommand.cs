//---------------------------------------------------------------------
// Authors: jachymko
//
// Description: Class implementing New-Junction command.
//
// Creation Date: Dec 13, 2006
//---------------------------------------------------------------------

using Microsoft.PowerShell.Commands;
using Pscx.Core.IO;
using Pscx.Win.Fwk.IO.Ntfs;
using System;
using System.ComponentModel;
using System.IO;
using System.Management.Automation;

namespace Pscx.Win.Commands.IO.Ntfs {
    [ProviderConstraint(typeof(FileSystemProvider))]
    [Cmdlet(VerbsCommon.New, PscxWinNouns.Junction, SupportsShouldProcess = true),
     Description("Creates NTFS directory junctions.")]
    [RelatedLink(typeof(NewHardLinkCommand)), RelatedLink(typeof(GetMountPointCommand)), 
     RelatedLink(typeof(RemoveMountPointCommand)), RelatedLink(typeof(GetReparsePointCommand)), 
     RelatedLink(typeof(RemoveReparsePointCommand)), RelatedLink(typeof(NewSymlinkCommand))]
    public sealed class NewJunctionCommand : NewLinkCommandBase
    {
        protected override Boolean OnValidatePscxPath(String parameterName, IPscxPathSettings settings)
        {
            if (parameterName == "TargetPath")
            {
                settings.PathType = PscxPathType.Container;

                return true;
            }

            return base.OnValidatePscxPath(parameterName, settings);
        }

        protected override void ProcessRecord()
        {
            if (ShouldProcess(TargetPath.ProviderPath,
                String.Format((string)"New NTFS junction via {0}", (object)LiteralPath.ProviderPath)))
            {
                Directory.CreateDirectory(LiteralPath.ProviderPath);

                if (!ReparsePointHelper.CreateJunction(LiteralPath.ProviderPath, TargetPath.ProviderPath))
                {
                    ErrorHandler.WriteLastWin32Error("CreateJunctionFailed", LiteralPath);
                }
                else
                {
                    WriteObject(new DirectoryInfo(LiteralPath.ProviderPath));
                }
            }
        }
    }
}
