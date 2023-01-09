//---------------------------------------------------------------------
// Author: jachymko
//
// Description: The Get-TerminalSession command.
//
// Creation Date: Jan 25 2007
//---------------------------------------------------------------------

using Pscx.Win.Fwk.TerminalServices;
using System.Management.Automation;

namespace Pscx.Win.Commands.TerminalServices
{
    [Cmdlet(VerbsCommon.Get, PscxWinNouns.TerminalSession)]
    public class GetTerminalSessionCommand : TerminalSessionCommandBase
    {
        [Parameter]
        public SwitchParameter Resolve
        {
            get;
            set;
        }

        protected override void ProcessSession(TerminalSession session)
        {
            if (Resolve && (session.Client != null))
            {
                session.Client.GetHostEntry();
            }

            WriteObject(session);
        }
    }
}
