//---------------------------------------------------------------------
// Authors: AlexAngelopoulos, Keith Hill, jachymko
//
// Description: Class implementing New-HardLink command.
//
// Creation Date: Oct  9, 2006
// Modified Date: Dec 13, 2006
//---------------------------------------------------------------------

using Pscx.Core.IO;
using Pscx.Win.Interop;
using System;
using System.ComponentModel;
using System.IO;
using System.Management.Automation;

namespace Pscx.Win.Commands.IO.Ntfs
{
    [Cmdlet(VerbsCommon.New, PscxWinNouns.Hardlink, SupportsShouldProcess = true),
     Description("Creates filesystem hard links. The hardlink and the target must reside on the same NTFS volume.")]
    [RelatedLink(typeof(NewJunctionCommand)), RelatedLink(typeof(GetMountPointCommand)), 
     RelatedLink(typeof(RemoveMountPointCommand)),RelatedLink(typeof(GetReparsePointCommand)), 
     RelatedLink(typeof(RemoveReparsePointCommand)), RelatedLink(typeof(NewSymlinkCommand))]
    public sealed class NewHardLinkCommand : NewLinkCommandBase
    {
        protected override Boolean OnValidatePscxPath(String parameterName, IPscxPathSettings settings)
        {
            if (parameterName == "TargetPath")
            {
                settings.PathType = PscxPathType.Leaf;
                return true;
            }

            return base.OnValidatePscxPath(parameterName, settings);
        }

        protected override void ProcessRecord()
        {
            if (ShouldProcess(TargetPath.ProviderPath, String.Format(
                (string)"Hard Link from {0}", (object)LiteralPath.ProviderPath)))
            {
                if (!NativeMethods.CreateHardLink(LiteralPath.ProviderPath, TargetPath.ProviderPath, IntPtr.Zero))
                {
                    ErrorHandler.WriteLastWin32Error("CreateHardLinkError", LiteralPath.ProviderPath);
                    return;
                }
                WriteObject(new FileInfo(LiteralPath.ProviderPath));
            }
        }
    }
}
