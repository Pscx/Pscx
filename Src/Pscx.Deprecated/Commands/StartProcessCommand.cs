//---------------------------------------------------------------------
// Author: Alex K. Angelopoulos & Keith Hill & jachymko
//
// Description: Start a new process.
//
// Creation Date: 2006-07-01
//      jachymko: 2007-06-21: added ComputerName parameter
//---------------------------------------------------------------------
using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Management.Automation;
using Pscx.Commands;

namespace Pscx.Deprecated.Commands
{
    [Cmdlet(VerbsLifecycle.Start, PscxNouns.Process, DefaultParameterSetName = ParameterSetPath, SupportsShouldProcess = true)]
    [Description("Starts a new process.")]
    [DetailedDescription("Starts a new process.  This cmdlet can be used with documents to invoke their default verb or the verb can be specified using the -Verb parameter.  You can also specify the -Credential parameter to run the process as a different user.")]
    [RelatedLink("Get-Process")]
    [RelatedLink("Stop-Processs")]
    public partial class StartProcessCommand : PscxCmdlet
    {
        internal const string ParameterSetPath = "Path";
        internal const string ParameterSetScriptBlock = "ScriptBlock";

        private ScriptBlock _scriptBlock;
        private PSCredential _credentials;
        private SwitchParameter _createNoWindow;
        private SwitchParameter _noShellExecute;
        private SwitchParameter _loadUserProfile;
        private SwitchParameter _noPowerShellProfile;
        private SwitchParameter _boost;
        private string _fileName;
        private string _workingDir;
        private bool _workingDirSpecified;
        private string _arguments = String.Empty;
        private string _verb = String.Empty;
        private int? _waitForExitTimeout;
        private ProcessWindowStyle _windowStyle = ProcessWindowStyle.Normal;
        private ProcessPriorityClass _priority = ProcessPriorityClass.Normal;
        private string[] _computers;
        private IDictionary _environment;

        #region Parameter declarations

        [Parameter(Position = 0, ParameterSetName = ParameterSetScriptBlock)]
        public ScriptBlock ScriptBlock
        {
            get { return _scriptBlock; }
            set { _scriptBlock = value; }
        }

        [Parameter(ParameterSetName = ParameterSetScriptBlock,
                   HelpMessage = "For scriptblocks that get executed in another PowerShell process, do not " +
                                 "load the PowerShell profile for that PowerShell process.")]
        public SwitchParameter NoProfile
        {
            get { return _noPowerShellProfile; }
            set { _noPowerShellProfile = value; }
        }

        [Alias("FileName", "FullName"),
         Parameter(Position = 0, ParameterSetName = ParameterSetPath, ValueFromPipelineByPropertyName = true,
                   HelpMessage = "Name of application if it is in the path, otherwise path to application or document or protocol string e.g. http://pscx.codeplex.com.")]
        public string Path
        {
            get { return _fileName; }
            set { _fileName = value; }
        }

        [Parameter(Position = 1, ParameterSetName = ParameterSetPath,
                   HelpMessage = "Optional string of arguments supplied to the process.")]
        public string Arguments
        {
            get { return _arguments; }
            set { _arguments = value; }
        }

        [Parameter(ParameterSetName = ParameterSetPath,
                   HelpMessage = "Invoke an item specific verb.")]
        public string Verb
        {
            get { return _verb; }
            set { _verb = value; }
        }

        [Parameter(HelpMessage = "The initial directory for the process to be started.")]
        public string WorkingDirectory
        {
            get { return _workingDir; }
            set 
            {
                _workingDir = value;
                _workingDirSpecified = true;
            }
        }

        [Credential,
         ValidateNotNull,
         Parameter(HelpMessage = "Specifies the credentials under which to run the process that is started.")]
        public PSCredential Credential
        {
            get { return _credentials; }
            set { _credentials = value; }
        }

        [Parameter(HelpMessage = "Do not use the operating system shell when starting the process.")]
        public SwitchParameter NoShellExecute
        {
            get { return _noShellExecute; }
            set { _noShellExecute = value; }
        }

        [Parameter(HelpMessage = "Do not create a window for this process, even if it normally uses one.")]
        public SwitchParameter NoWindow
        {
            get { return _createNoWindow; }
            set { _createNoWindow = value; }
        }

        [Parameter(HelpMessage = "Style of display for the application window if one is shown."),
         DefaultValue("Normal")]
        public ProcessWindowStyle WindowStyle
        {
            get { return _windowStyle; }
            set { _windowStyle = value; }
        }

        [Parameter(HelpMessage = "Loads the user's profile from the HKEY_USERS registry key. Loading the " +
                                 "profile can be time-consuming, so it is best to use this value only if " +
                                 "you must access the information in the HKEY_CURRENT_USER registry key.")]
        public SwitchParameter LoadUserProfile
        {
            get { return _loadUserProfile; }
            set { _loadUserProfile = value; }
        }

        [ValidateRange(-1, int.MaxValue),
         Parameter(HelpMessage = "Wait until the process exits or the time in seconds expires before " +
                                 "returning.  Use -1 seconds for infinite timeout."),
         DefaultValue("N/A")]
        public int WaitTimeout
        {
            get { return _waitForExitTimeout ?? 0; }
            set { _waitForExitTimeout = value; }
        }

        [Alias("PriorityBoostEnabled"),
         Parameter(HelpMessage = "Gets or sets a value indicating whether the associated process _priority " +
                                 "should temporarily be boosted when the main window has the focus.")]
        public SwitchParameter Boost
        {
            get { return _boost; }
            set { _boost = value; }
        }

        [Alias("PriorityClass"),
         Parameter(HelpMessage = "Indicates the priority that the system associates with a process. This value, " +
                                 "together with the priority value of each thread of the process, determines each thread's base priority level."),
         DefaultValue("Normal")]
        public ProcessPriorityClass Priority
        {
            get { return _priority; }
            set { _priority = value; }
        }

        [Parameter]
        public string[] ComputerName
        {
            get { return _computers; }
            set { _computers = value; }
        }

        [Parameter]
        public IDictionary Environment
        {
            get { return _environment; }
            set { _environment = value; }
        }

        #endregion Parameters

        protected override void BeginProcessing()
        {
            string warning = String.Format(Properties.Resources.DeprecatedCmdlet_F2, CmdletName, @"PowerShell's built-in Microsoft.PowerShell.Management\Start-Process cmdlet");
            WriteWarning(warning);

            if (WildcardPattern.ContainsWildcardCharacters(_fileName))
            {
                ErrorHandler.ThrowIllegalCharsInPath(_fileName);
            }

            if (_credentials != null)
            {
                // If using credentials, NoShellExecute must be set to true
                _noShellExecute = true;
            }

            if (_workingDir == null)
            {
                _workingDir = SessionState.Path.CurrentFileSystemLocation.Path;
            }
        }

        protected override void ProcessRecord()
        {
            ProcessLauncherBase launcher;

            if (_computers != null && _computers.Length > 0)
            {
                launcher = new RemoteProcessLauncher(this, _computers);
            }
            else
            {
                launcher = new LocalProcessLauncher(this);
            }

            ProcessStartInfo startInfo;

            if (ParameterSetName == ParameterSetScriptBlock)
            {
                startInfo = CreateProcessStartInfoForScriptBlock();
            }
            else
            {
                startInfo = CreateProcessStartInfo();
            }

            if (_environment != null)
            {
                startInfo.EnvironmentVariables.Clear();

                foreach (object key in _environment.Keys)
                {
                    startInfo.EnvironmentVariables[key.ToString()] = _environment[key].ToString();
                }
            }

            startInfo.Verb = _verb;
            startInfo.UseShellExecute = !_noShellExecute;
            startInfo.CreateNoWindow = _createNoWindow;
            startInfo.LoadUserProfile = _loadUserProfile;
            startInfo.WindowStyle = _windowStyle;
            startInfo.WorkingDirectory = _workingDir;

            if (_credentials != null)
            {
                startInfo.Domain = _credentials.GetNetworkCredential().Domain;
                startInfo.UserName = _credentials.GetNetworkCredential().UserName;
                startInfo.Password = _credentials.Password;
            }

            Process[] processes = launcher.Start(startInfo);
            WriteObject(processes, true);
        }

        private ProcessStartInfo CreateProcessStartInfo()
        {
            return new ProcessStartInfo(_fileName, _arguments);
        }

        private ProcessStartInfo CreateProcessStartInfoForScriptBlock()
        {
            string args = "-NoLogo -NoExit ";
            if (_noPowerShellProfile)
            {
                args += "-NoProfile ";
            }
            args += "-Command " + _scriptBlock.ToString();

            return new ProcessStartInfo("powershell.exe", args);
        }

        private abstract class ProcessLauncherBase
        {
            private readonly StartProcessCommand _command;

            protected ProcessLauncherBase(StartProcessCommand command)
            {
                _command = command;
            }

            protected StartProcessCommand Command
            {
                get { return _command; }
            }

            public abstract Process[] Start(ProcessStartInfo psi);
        }
    }
}
