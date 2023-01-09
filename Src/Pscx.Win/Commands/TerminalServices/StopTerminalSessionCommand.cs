//---------------------------------------------------------------------
// Author: jachymko
//
// Description: Logs off a terminal session.
//
// Creation Date: 24 Jan 2007
//---------------------------------------------------------------------

using Pscx.Win.Fwk.TerminalServices;
using System.ComponentModel;
using System.Management.Automation;

namespace Pscx.Win.Commands.TerminalServices
{
    [Cmdlet(VerbsLifecycle.Stop, PscxWinNouns.TerminalSession, ConfirmImpact = ConfirmImpact.High, SupportsShouldProcess = true)]
    [Description("Logs off a specific remote desktop session on a system running Terminal Services/Remote Desktop")]
    public class StopTerminalSessionCommand : TerminalSessionCommandBase
    {
        private SwitchParameter _force;

        [Parameter]
        public SwitchParameter Force
        {
            get { return _force; }
            set { _force = value;  }
        }

        protected override void ProcessSession(TerminalSession session)
        {
            if (_force.IsPresent || ShouldContinue("Logoff the session " + session.Id + "?", CmdletName))
            {
                session.Logoff(ShouldWait);
            }
        }
    }
}