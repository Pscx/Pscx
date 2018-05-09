//---------------------------------------------------------------------
// Authors: AlexAngelopoulos, Keith Hill, jachymko
//
// Description: Class implementing Get-MountPoint command.
//
// Creation Date: Oct  9, 2006
// Modified Date: Dec 16, 2006
//---------------------------------------------------------------------
using System;
using System.ComponentModel;
using System.Management.Automation;
using Microsoft.PowerShell.Commands;
using Pscx.Interop;
using Pscx.IO;
using Pscx.IO.Ntfs;

namespace Pscx.Commands.IO.Ntfs
{
    [ProviderConstraint(typeof(FileSystemProvider))]
    [Cmdlet(VerbsCommon.Remove, PscxNouns.MountPoint, DefaultParameterSetName = ParameterSetPath)]
    [Description("Removes a mount point, dismounting the current media if any. If used against the root of a fixed drive, removes the drive letter assignment.")]
    [RelatedLink(typeof(NewHardLinkCommand)), RelatedLink(typeof(NewJunctionCommand)),
     RelatedLink(typeof(GetMountPointCommand)), RelatedLink(typeof(GetReparsePointCommand)), 
     RelatedLink(typeof(RemoveReparsePointCommand)), RelatedLink(typeof(NewSymlinkCommand))]
    public sealed class RemoveMountPointCommand : PscxPathCommandBase
    {
        protected override void OnValidateLiteralPath(IPscxPathSettings settings)
        {
            settings.PathType = PscxPathType.Container;
            base.OnValidateLiteralPath(settings);
        }

        protected override void OnValidatePath(IPscxPathSettings settings)
        {
            settings.PathType = PscxPathType.Container;
            base.OnValidatePath(settings);
        }

        protected override void ProcessPath(PscxPathInfo pscxPath)
        {
            var name = ReparsePointHelper.EnsurePathSlash(pscxPath.ProviderPath);

            if (!NativeMethods.DeleteVolumeMountPoint(name))
            {
                ErrorHandler.WriteLastWin32Error("DeleteVolumeMountPointFailed", name);
            }

            base.ProcessPath(pscxPath);
        }
    }
}
