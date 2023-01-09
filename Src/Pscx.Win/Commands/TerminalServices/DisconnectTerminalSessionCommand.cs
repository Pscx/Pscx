//---------------------------------------------------------------------
// Author: jachymko
//
// Description: Disconnects a terminal session.
//
// Creation Date: 27 Jan 2007
//---------------------------------------------------------------------

using Pscx.Win.Fwk.TerminalServices;
using System.ComponentModel;
using System.Management.Automation;

namespace Pscx.Win.Commands.TerminalServices
{
    [Cmdlet(VerbsCommunications.Disconnect, PscxWinNouns.TerminalSession, SupportsShouldProcess = true)]
    [Description("Disconnects a specific remote desktop session on a system running Terminal Services/Remote Desktop")]
    public class DisconnectTerminalSessionCommand : TerminalSessionCommandBase
    {
        protected override void ProcessSession(TerminalSession session)
        {
            session.Disconnect(ShouldWait);
        }
    }
}