//---------------------------------------------------------------------
// Author: Alex K. Angelopoulos & Keith Hill & jachymko
//
// Description: Start a new process.
//
// Creation Date: 2006-07-01
//      jachymko: 2007-06-21: added ComputerName parameter
//---------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Management;
using System.Text;
using Pscx.Commands;

namespace Pscx.Deprecated.Commands
{
    partial class StartProcessCommand : PscxCmdlet
    {
        private sealed class RemoteProcessLauncher : ProcessLauncherBase
        {
            private readonly string[] _computers;

            private ManagementScope _scope;
            private bool _environmentWarning;

            public RemoteProcessLauncher(StartProcessCommand cmd, string[] comupterNames)
                : base(cmd)
            {
                _computers = comupterNames;
            }

            public override Process[] Start(ProcessStartInfo psi)
            {
                List<Process> processes = new List<Process>();

                ConnectionOptions options = GetConnectionOptions();

                foreach (string comp in _computers)
                {
                    _scope = GetScope(comp, options);
                    _scope.Connect();

                    try
                    {
                        Process process = CreateProcess(psi);
                        
                        if (process != null)
                        {
                            processes.Add(process);
                        }
                    }
                    finally
                    {
                        _scope = null;
                    }
                }

                return processes.ToArray();
            }

            private ManagementObject CreateProcessStartup(ProcessStartInfo psi)
            {
                ManagementClass win32_ProcessStartup = GetClass(_scope, "Win32_ProcessStartup");
                ManagementObject instance = win32_ProcessStartup.CreateInstance();
                if (Command.Environment != null && Command.Environment.Count > 0)
                {
                    if (!_environmentWarning)
                    {
                        Command.WriteWarning(Properties.Resources.StartProcessRemoteEnvironment);
                        _environmentWarning = true;
                    }
                    instance["EnvironmentVariables"] = BuildEnvironment(psi.EnvironmentVariables);
                }

                instance["PriorityClass"] = Command.Priority;
                
                return instance;
            }

            private Process CreateProcess(ProcessStartInfo psi)
            {
                ManagementClass win32_Process = GetClass(_scope, "Win32_Process");
                ManagementBaseObject inParams = win32_Process.GetMethodParameters("Create");

                inParams["CommandLine"] = BuildCommandLine(psi.FileName, psi.Arguments);
                inParams["ProcessStartupInformation"] = CreateProcessStartup(psi);

                if (Command._workingDirSpecified)
                {
                    inParams["CurrentDirectory"] = psi.WorkingDirectory;
                }

                ManagementBaseObject outParams = win32_Process.InvokeMethod("Create", inParams, 
                    new InvokeMethodOptions());

                CreateProcessResult returnValue = (CreateProcessResult)(uint)outParams["ReturnValue"];

                if (returnValue == CreateProcessResult.Success)
                {
                    int processId = unchecked((int)(uint)outParams["ProcessId"]);
                    Process process = null;

                    try
                    {
                        process = Process.GetProcessById(processId, _scope.Path.Server);
                    }
                    catch (ArgumentException)
                    {
                        Command.WriteWarning(string.Format(Properties.Resources.StartProcessExitedTooSoon, processId));
                    }

                    return process;
                }
                else
                {
                    HandleError(psi, returnValue);
                }

                return null;
            }

            private ManagementScope GetScope(string computer, ConnectionOptions options)
            {
                string path = string.Format("\\\\{0}\\ROOT\\CIMV2", computer);

                return new ManagementScope(path, options);
            }

            private ManagementClass GetClass(ManagementScope scope, string className)
            {
                ManagementPath path = new ManagementPath(className);
                
                return new ManagementClass(scope, path, new ObjectGetOptions());
            }

            private ConnectionOptions GetConnectionOptions()
            {
                ConnectionOptions options = new ConnectionOptions();

                if (Command.Credential != null)
                {
                    options.Username = Command.Credential.UserName;
                    // TODO: Commented out because it doesn't compile: options.SecurePassword = Command.Credential.Password;
                }

                return options;
            }

            private void HandleError(ProcessStartInfo psi, CreateProcessResult returnValue)
            {
                switch (returnValue)
                {
                    case CreateProcessResult.AccessDenied:
                    case CreateProcessResult.InsufficientPrivilege:
                        Command.ErrorHandler.WriteFileError(psi.FileName, new UnauthorizedAccessException());
                        break;

                    case CreateProcessResult.PathNotFound:
                        Command.ErrorHandler.WriteFileNotFoundError(psi.FileName);
                        break;

                    case CreateProcessResult.UnknownFailure:
                    default:
                        string message = string.Format(Properties.Resources.StartProcessFailed, psi.FileName, _scope.Path.Server);
                        Command.ErrorHandler.WriteProcessFailedToStart(null, new Exception(message));
                        break;

                }
            }

            private static string BuildCommandLine(string fileName, string arguments)
            {
                fileName = fileName.Trim();

                bool quoted = fileName.StartsWith("\"", StringComparison.Ordinal) &&
                                fileName.EndsWith("\"", StringComparison.Ordinal);

                StringBuilder builder = new StringBuilder();

                if (!quoted)
                {
                    builder.Append("\"");
                }

                builder.Append(fileName);

                if (!quoted)
                {
                    builder.Append("\"");
                }

                if (!string.IsNullOrEmpty(arguments))
                {
                    builder.Append(" ");
                    builder.Append(arguments);
                }

                return builder.ToString();
            }

            private static string[] BuildEnvironment(StringDictionary env)
            {
                int index = 0;
                string[] strings = new string[env.Count];

                foreach (string key in env.Keys)
                {
                    strings[index] = string.Concat(key, "=", env[key]);
                    index++;
                }

                return strings;
            }

            private enum CreateProcessResult : uint
            {
                Success = 0,
                AccessDenied = 2,
                InsufficientPrivilege = 3,
                UnknownFailure = 8,
                PathNotFound = 9,
            }
        }
    }
}
