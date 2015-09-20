//---------------------------------------------------------------------
// Authors: jachymko
//
// Description: Class implementing the New-Shortcut command.
//
// Creation Date: Dec 13, 2006
//---------------------------------------------------------------------
using System;
using System.ComponentModel;
using System.IO;
using System.Management.Automation;
using Pscx.Interop;

namespace Pscx.Commands.IO
{
    [Cmdlet(VerbsCommon.New, PscxNouns.Shortcut, SupportsShouldProcess = true)]
    [OutputType(new[] {typeof(FileInfo)})]
    [Description("Creates shell shortcuts.")]
    public partial class NewShortcutCommand : NewLinkCommandBase
    {
        protected override void ProcessRecord()
        {
            try
            {
                var linkFile = Path.ChangeExtension(LiteralPath.ProviderPath, ".lnk");

                if (ShouldProcess(TargetPath.ProviderPath,
                    String.Format("New shortcut via {0}", LiteralPath.ProviderPath)))
                {
                    var link = (IShellLink) new CShellLink();
                    var persist = (IPersistFile) (link);

                    link.SetPath(TargetPath.ProviderPath);
                    persist.Save(linkFile, false);

                    WriteObject(new FileInfo(linkFile));
                }
            }
            catch (PipelineStoppedException)
            {
                throw;
            }
            catch (Exception exc)
            {
                WriteError(new ErrorRecord(exc, "CreateShellLinkError", ErrorCategory.NotSpecified, LiteralPath));
            }
        }
    }
}
