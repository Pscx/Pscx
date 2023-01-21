//---------------------------------------------------------------------
// Authors: jachymko
//
// Description: Class implementing the New-SymLink command.
//
// Creation Date: Dec 13, 2006
//---------------------------------------------------------------------

using Microsoft.PowerShell.Commands;
using Pscx.Commands;
using Pscx.Core.IO;
using Pscx.Win.Interop;
using System;
using System.ComponentModel;
using System.IO;
using System.Management.Automation;

namespace Pscx.Win.Commands.IO.Ntfs {
    [ProviderConstraint(typeof(FileSystemProvider))]
    [Cmdlet(VerbsCommon.Remove, PscxWinNouns.ReparsePoint, SupportsShouldProcess = true, DefaultParameterSetName = ParameterSetPath),
     Description("Removes NTFS reparse junctions and symbolic links.")]
    [RelatedLink(typeof(NewHardLinkCommand)), RelatedLink(typeof(NewJunctionCommand)),
     RelatedLink(typeof(GetMountPointCommand)), RelatedLink(typeof(RemoveMountPointCommand)),
     RelatedLink(typeof(GetReparsePointCommand)), RelatedLink(typeof(NewSymlinkCommand))]
    public partial class RemoveReparsePointCommand : PscxPathCommandBase {
        protected override Boolean OnValidatePscxPath(String parameterName, IPscxPathSettings settings) {
            settings.ShouldExist = true;
            return base.OnValidatePscxPath(parameterName, settings);
        }

        protected override void ProcessPath(PscxPathInfo pscxPath) {
            if (ShouldProcess(pscxPath.ProviderPath)) {
                var fi = Core.Utils.GetFileOrDirectory(pscxPath.ProviderPath);

                if ((fi.Attributes & FileAttributes.ReparsePoint) != FileAttributes.ReparsePoint) {
                    ErrorHandler.WriteIsNotReparsePointError(pscxPath.ProviderPath);
                    return;
                }

                if (!DeleteFileOrDirectory(pscxPath, fi)) {
                    ErrorHandler.WriteLastWin32Error("RemoveReparsePointFailed", Path);
                    return;
                }
            }

            base.ProcessPath(pscxPath);
        }

        private static bool DeleteFileOrDirectory(PscxPathInfo pscxPath, FileSystemInfo fi) {
            if (fi is DirectoryInfo) {
                return NativeMethods.RemoveDirectory(pscxPath.ProviderPath);
            }

            return NativeMethods.DeleteFile(pscxPath.ProviderPath);
        }
    }
}