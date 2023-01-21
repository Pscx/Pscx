//---------------------------------------------------------------------
// Authors: jachymko
//
// Description: Class implementing the New-Shortcut command.
//
// Creation Date: Dec 13, 2006
//---------------------------------------------------------------------

using Pscx.Core.IO;
using Pscx.Win.Interop.Shell;
using System;
using System.ComponentModel;
using System.IO;
using System.Management.Automation;

namespace Pscx.Win.Commands.IO
{
    [Cmdlet(VerbsCommon.New, PscxWinNouns.Shortcut, SupportsShouldProcess = true),
     Description("Creates shell shortcuts.")]
    [OutputType(new[] {typeof(FileInfo)})]
    public partial class NewShortcutCommand : NewLinkCommandBase
    {
        protected override void ProcessRecord()
        {
            try
            {
                var linkFile = Path.ChangeExtension(LiteralPath.ProviderPath, ".lnk");

                if (ShouldProcess(TargetPath.ProviderPath,
                    String.Format((string)"New shortcut via {0}", (object)LiteralPath.ProviderPath)))
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
