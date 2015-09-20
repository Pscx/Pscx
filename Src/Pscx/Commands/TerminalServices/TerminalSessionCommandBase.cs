//---------------------------------------------------------------------
// Author: jachymko
//
// Description: Base for commands processing terminal sessions.
//
// Creation Date: Jan 25 2007
//---------------------------------------------------------------------
using System;
using System.Management.Automation;

using Pscx.TerminalServices;

namespace Pscx.Commands.TerminalServices
{
    public abstract class TerminalSessionCommandBase : TerminalServerCommandBase
    {
        [Alias("SessionId")]
        [Parameter(Position = 1, Mandatory = false, ValueFromPipelineByPropertyName = true)]
        public Int32[] Id
        {
            get;
            set;
        }

        protected override void ProcessServer(TerminalServer server)
        {
            foreach (TerminalSession session in server.Sessions)
            {
                if (Id == null || Array.IndexOf<Int32>(Id, session.Id) > -1)
                {
                    try
                    {
                        ProcessSession(session);
                    }
                    catch (System.Runtime.InteropServices.ExternalException exc)
                    {
                        ErrorHandler.WriteWin32Error(exc, "TerminalSessionError", ErrorCategory.NotSpecified, session);
                    }
                }

                if (Stopping)
                {
                    break;
                }
            }
        }

        protected virtual void ProcessSession(TerminalSession session)
        {
        }
    }
}
