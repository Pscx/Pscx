//---------------------------------------------------------------------
// Author: Alex K. Angelopoulos & Keith Hill & jachymko
//
// Description: Start a new process.
//
// Creation Date: 2006-07-01
//      jachymko: 2007-06-21: added ComputerName parameter
//---------------------------------------------------------------------
using System;
using System.Diagnostics;
using Pscx.Commands;

namespace Pscx.Deprecated.Commands
{
    partial class StartProcessCommand : PscxCmdlet
    {
        private sealed class LocalProcessLauncher : ProcessLauncherBase
        {
            public LocalProcessLauncher(StartProcessCommand cmd)
                : base(cmd)
            {
            }

            public override Process[] Start(ProcessStartInfo startInfo)
            {
                Process process = null;

                try
                {
                    process = new Process();
                    process.StartInfo = startInfo;
                    
                    StartProcess(process);

                    return new Process[] { process };
                }
                catch (Exception ex)
                {
                    Command.ErrorHandler.WriteProcessFailedToStart(process, ex);
                }

                return new Process[0];
            }

            private void StartProcess(Process process)
            {
                ProcessStartInfo startInfo = process.StartInfo;

                string fmt = Properties.Resources.ShouldProcessStartProcess_F3;
                string msg = String.Format(fmt, startInfo.FileName, startInfo.Arguments, startInfo.WorkingDirectory);

                if (Command.ShouldProcess(msg))
                {
                    if (process.Start())
                    {
                        if (process.PriorityClass != Command._priority)
                        {
                            process.PriorityClass = Command._priority;
                        }

                        // On Vista we use -verb runas to elevate a process but if you try to set this
                        // property (even to false) an InvalidOperationException is thrown.  Since the
                        // default is false, there is no point in setting this unless _boost is true.
                        if (Command._boost)
                        {
                            process.PriorityBoostEnabled = Command._boost;
                        }

                        if (Command._waitForExitTimeout.HasValue)
                        {
                            if (Command._waitForExitTimeout.Value == -1)
                            {
                                // Infinite wait
                                process.WaitForExit();
                            }
                            else
                            {
                                if (!process.WaitForExit(Command._waitForExitTimeout.Value * 1000))
                                {
                                    Command.WriteWarning(String.Format(Properties.Resources.StartProcessTimeout, Command.CmdletName, process.Id));
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
