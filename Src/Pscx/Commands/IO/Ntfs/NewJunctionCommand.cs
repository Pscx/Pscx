//---------------------------------------------------------------------
// Authors: jachymko
//
// Description: Class implementing New-Junction command.
//
// Creation Date: Dec 13, 2006
//---------------------------------------------------------------------
using System.ComponentModel;
using System.IO;
using System.Management.Automation;
using Pscx.IO;
using Pscx.IO.Ntfs;
using System;
using Microsoft.PowerShell.Commands;

namespace Pscx.Commands.IO.Ntfs
{
    [ProviderConstraint(typeof(FileSystemProvider))]
    [Cmdlet(VerbsCommon.New, PscxNouns.Junction, SupportsShouldProcess = true)]
    [Description("Creates NTFS directory junctions.")]
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
                String.Format("New NTFS junction via {0}", LiteralPath.ProviderPath)))
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
