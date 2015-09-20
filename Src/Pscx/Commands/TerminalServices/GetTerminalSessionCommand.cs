//---------------------------------------------------------------------
// Author: jachymko
//
// Description: The Get-TerminalSession command.
//
// Creation Date: Jan 25 2007
//---------------------------------------------------------------------
using System;
using System.Management.Automation;

using Pscx.TerminalServices;

namespace Pscx.Commands.TerminalServices
{
    [Cmdlet(VerbsCommon.Get, PscxNouns.TerminalSession)]
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
