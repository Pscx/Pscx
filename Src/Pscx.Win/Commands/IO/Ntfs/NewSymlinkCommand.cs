//---------------------------------------------------------------------
// Authors: jachymko
//
// Description: Class implementing the New-SymLink command.
//
// Creation Date: Dec 13, 2006
//---------------------------------------------------------------------

using Pscx.Core.IO;
using Pscx.Win.Interop;
using System;
using System.ComponentModel;
using System.IO;
using System.Management.Automation;

namespace Pscx.Win.Commands.IO.Ntfs {
    [Cmdlet(VerbsCommon.New, PscxWinNouns.Symlink, SupportsShouldProcess = true),
     Description("Creates filesystem symbolic links. Requires Microsoft Windows 7 or later.")]
    [RelatedLink(typeof(NewHardLinkCommand)), RelatedLink(typeof(NewJunctionCommand)),
     RelatedLink(typeof(GetMountPointCommand)), RelatedLink(typeof(RemoveMountPointCommand)),
     RelatedLink(typeof(GetReparsePointCommand)), RelatedLink(typeof(RemoveReparsePointCommand))]
    public partial class NewSymlinkCommand : NewLinkCommandBase {
        protected override void BeginProcessing() {
            base.BeginProcessing();

            EnsureDependency(PscxDependency.WindowsVista);
        }

        protected override void ProcessRecord() {
            var target = Core.Utils.GetFileOrDirectory(TargetPath.ProviderPath);
            var flags = 0;

            if (target is DirectoryInfo) {
                flags = NativeMethods.SYMLINK_FLAG_DIRECTORY;
            }

            if (ShouldProcess(TargetPath.ProviderPath, String.Format((string)"New symbolic link via {0}", (object)LiteralPath.ProviderPath))) {
                if (!NativeMethods.CreateSymbolicLink(LiteralPath.ProviderPath, TargetPath.ProviderPath, flags)) {
                    ErrorHandler.WriteLastWin32Error("CreateSymbolicLinkError", LiteralPath);
                    return;
                }

                WriteObject(Core.Utils.GetFileOrDirectory(LiteralPath.ProviderPath));
            }
        }
    }
}
