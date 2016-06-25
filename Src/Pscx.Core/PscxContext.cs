//---------------------------------------------------------------------
// Author: jachymko, Keith Hill
//
// Description: Single container for all PSCX variables including 
//              preference and session variables.
//
// Creation Date: Aug 10, 2008
//---------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management.Automation;
using System.Security.Principal;
using Pscx.EnvironmentBlock;
using Pscx.Interop;

namespace Pscx
{
    public sealed class PscxContext
    {
        public const string EditFileBackingFileThreshold = "EditFileBackingFileThreshold";
        public const int EditFileBackingFileThresholdDefaultValue = 84000;
        private static readonly PscxContext _instance = new PscxContext();
        private WindowsIdentity _identity;
        private WindowsPrincipal _principal;
        private Stack<EnvironmentFrame> _environment;

        private PscxContext()
        {
            this.Preferences = new Hashtable(StringComparer.OrdinalIgnoreCase);
            this.Session     = new Hashtable(StringComparer.OrdinalIgnoreCase);
            this.Home        = Path.GetDirectoryName(GetType().Assembly.Location);
            this.AppsDir     = Path.Combine(this.Home, "Apps");

            using (var variable = new PathVariable("Path", EnvironmentVariableTarget.Process))
            {
                variable.Append(this.AppsDir);
            }

            this.Is64BitProcess = (IntPtr.Size == 8);
            this.IsWow64Process = GetIsWow64Process() ?? false;

            InitializePscxPreferences();
        }

        private void InitializePscxPreferences()
        {
            this.Preferences["ShowModuleLoadDetails"]      = false;
            this.Preferences["CD_GetChildItem"]            = false;
            this.Preferences["CD_EchoNewLocation"]         = false;
            this.Preferences[EditFileBackingFileThreshold] = EditFileBackingFileThresholdDefaultValue;
            this.Preferences["FileSizeInUnits"]            = false;
            this.Preferences["PageHelpUsingLess"]          = true;
            this.Preferences["PromptTheme"]                = "Modern";
            this.Preferences["TextEditor"]                 = "Notepad.exe";
            this.Preferences["SmtpFrom"]                   = null;
            this.Preferences["SmtpHost"]                   = null;
            this.Preferences["SmtpPort"]                   = null;

            var modulesToImport = new Hashtable(StringComparer.OrdinalIgnoreCase);
            modulesToImport["CD"] = true;
            modulesToImport["DirectoryServices"] = false;
            modulesToImport["FileSystem"]        = false;
            modulesToImport["GetHelp"]           = false;
            modulesToImport["Net"]               = false;
            modulesToImport["Prompt"]            = false;
            modulesToImport["TranscribeSession"] = false;
            modulesToImport["Utility"]           = true;
            modulesToImport["Vhd"]               = false;
            modulesToImport["Wmi"]               = false;
            this.Preferences["ModulesToImport"]  = modulesToImport;
        }

        public bool      Is64BitProcess { get; private set; }
        public bool      IsWow64Process { get; private set; }
        public Hashtable Preferences    { get; private set; }
        public Hashtable Session        { get; private set; }
        public string    Home           { get; private set; }
        public string    AppsDir        { get; private set; }

        public static PscxContext Instance
        {
            get { return _instance;  }   
        }

        public static PSObject InstanceAsPSObject
        {
            get { return PSObject.AsPSObject(_instance); }
        }

        public Version Version
        {
            get { return GetType().Assembly.GetName().Version; }
        }

        public NTAccount WindowsAccount
        {
            get { return (NTAccount)WindowsIdentity.User.Translate(typeof(NTAccount)); }
        }

        public WindowsIdentity WindowsIdentity
        {
            get 
            {
                if (_identity == null)
                {
                    _identity = WindowsIdentity.GetCurrent();
                }

                return _identity;
            }
        }

        public WindowsPrincipal WindowsPrincipal
        {
            get
            {
                if (_principal == null)
                {
                    _principal = new WindowsPrincipal(WindowsIdentity);
                }

                return _principal;
            }
        }

        public bool IsAdmin
        {
            get { return WindowsPrincipal.IsInRole(WindowsBuiltInRole.Administrator);  }
        }

        private bool? GetIsWow64Process()
        {
            try
            {
                UIntPtr proc = NativeMethods.GetProcAddress(NativeMethods.GetModuleHandle("Kernel32.dll"), "IsWow64Process");
                if (proc == UIntPtr.Zero) return null;  // I can't find the answer to this query

                bool retval;
                if (!NativeMethods.IsWow64Process(NativeMethods.GetCurrentProcess(), out retval))
                {
                    return null;
                }

                return retval;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(String.Format("Failed to determine IsWow64Process: {0}", ex));
                return null;
            }
        }

        internal Stack<EnvironmentFrame> EnvironmentStack
        {
            get
            {
                if (_environment == null)
                {
                    _environment = new Stack<EnvironmentFrame>();
                }

                return _environment;
            }
        }
    }
}
