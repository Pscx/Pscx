//---------------------------------------------------------------------
// Author: jachymko
//
// Description: Base for commands processing terminal servers.
//
// Creation Date: Jan 25 2007
//---------------------------------------------------------------------
using System;
using System.Management.Automation;


using Pscx.TerminalServices;

namespace Pscx.Commands.TerminalServices
{
    public abstract class TerminalServerCommandBase : PscxCmdlet
    {
        private bool _processLocal = true;

        private string[] _computer;
        private SwitchParameter _wait;

        [Parameter(Position = 0, Mandatory = false, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        public string[] ComputerName
        {
            get { return _computer; }
            set { _computer = value; }
        } 

        [Parameter(HelpMessage = "Wait for results before returning.")]
        public SwitchParameter Wait
        {
            get { return _wait; }
            set { _wait = value; }
        }

        protected override void ProcessRecord()
        {
            if (_computer != null)
            {
                foreach (string comp in _computer)
                {
                    _processLocal = false;

                    if (comp.Trim() == ".")
                    {
                        ProcessLocalServer();
                    }
                    else
                    {
                        ProcessServer(comp);
                    }

                    if (Stopping)
                    {
                        break;
                    }
                }
            }

            base.ProcessRecord();
        }

        protected override void EndProcessing()
        {
            if (_processLocal)
            {
                ProcessServer(new TerminalServer());
            }

            base.EndProcessing();
        }

        protected virtual void ProcessServer(TerminalServer server)
        {
        }

        protected bool ShouldWait 
        {
            get { return _wait.IsPresent; } 
        }

        private void ProcessLocalServer()
        {
            try
            {
                ProcessServer(new TerminalServer());
            }
            catch (System.Runtime.InteropServices.ExternalException exc)
            {
                ErrorHandler.WriteWin32Error(exc, "TerminalServerError", ErrorCategory.NotSpecified, "localhost");
            }
        }

        private void ProcessServer(string serverName)
        {
            try
            {
                ProcessServer(new TerminalServer(serverName));
            }
            catch (UnauthorizedAccessException exc)
            {
                ErrorHandler.HandleUnauthorizedAccessError(false, serverName, exc);
            }
            catch (System.Runtime.InteropServices.ExternalException exc)
            {
                ErrorHandler.WriteWin32Error(exc, "TerminalServerError", ErrorCategory.NotSpecified, serverName);
            }
        }
    }
}
