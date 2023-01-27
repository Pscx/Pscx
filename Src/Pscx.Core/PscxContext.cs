//---------------------------------------------------------------------
// Author: jachymko, Keith Hill
//
// Description: Single container for all PSCX variables including 
//              preference and session variables.
//
// Creation Date: Aug 10, 2008
//---------------------------------------------------------------------

using Pscx.EnvironmentBlock;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management.Automation;
using System.Runtime.Versioning;
using System.Security.Principal;

namespace Pscx.Core {
    public sealed class PscxContext {
        public const string EditFileBackingFileThreshold = "EditFileBackingFileThreshold";
        public const int EditFileBackingFileThresholdDefaultValue = 84000;
        private static readonly PscxContext _instance = new();
        private WindowsIdentity _identity;
        private WindowsPrincipal _principal;
        private Stack<EnvironmentFrame> _environment;

        private PscxContext() {
            Preferences = new Hashtable(StringComparer.OrdinalIgnoreCase);
            Session = new Hashtable(StringComparer.OrdinalIgnoreCase);
            Home = Path.GetDirectoryName(GetType().Assembly.Location);
            AppsDir = Path.Combine(this.Home, "Apps");
            if (OperatingSystem.IsWindows()) {
                AppsDir = Path.Combine(AppsDir, "Win");
            } else if (OperatingSystem.IsLinux()) {
                AppsDir = Path.Combine(AppsDir, "Linux");
            } else if (OperatingSystem.IsMacOS()) {
                AppsDir = Path.Combine(AppsDir, "macOS");
            }

            using (var variable = new PathVariable("Path", EnvironmentVariableTarget.Process)) {
                variable.Append(this.AppsDir);
            }

            this.Is64BitProcess = (IntPtr.Size == 8);

            InitializePscxPreferences();
        }

        private void InitializePscxPreferences() {
            this.Preferences["ShowModuleLoadDetails"] = false;
            this.Preferences["CD_GetChildItem"] = false;
            this.Preferences["CD_EchoNewLocation"] = false;
            this.Preferences[EditFileBackingFileThreshold] = EditFileBackingFileThresholdDefaultValue;
            this.Preferences["FileSizeInUnits"] = false;
            this.Preferences["PageHelpUsingLess"] = true;
            this.Preferences["TextEditor"] = DefaultTextEditor;

            var modulesToImport = new Hashtable(StringComparer.OrdinalIgnoreCase) {
                { "CD", true },
                { "DirectoryServices", false }, //from PscxWin submodule
                { "FileSystem", false},
                { "Net", true},
                { "TranscribeSession", false},
                { "Utility", true},
                { "Vhd", false},  //from PscxWin submodule
                { "Wmi", false},  //from PscxWin submodule
                { "Sudo", (OperatingSystem.IsWindows())}
            };
            this.Preferences["ModulesToImport"] = modulesToImport;
        }

        public bool Is64BitProcess { get; private set; }
        public Hashtable Preferences { get; private set; }
        public Hashtable Session { get; private set; }
        public string Home { get; private set; }
        public string AppsDir { get; private set; }

        public static PscxContext Instance {
            get { return _instance; }
        }

        public static PSObject InstanceAsPSObject {
            get { return PSObject.AsPSObject(_instance); }
        }

        public static string DefaultTextEditor {
            get { return OperatingSystem.IsWindows() ? "Notepad.exe" : (OperatingSystem.IsMacOS() ? "TextEdit" : "gedit"); }
        }

        public Version Version {
            get { return GetType().Assembly.GetName().Version; }
        }

        [SupportedOSPlatform("windows")]
        public NTAccount WindowsAccount {
            get { return (NTAccount)WindowsIdentity.User.Translate(typeof(NTAccount)); }
        }

        [SupportedOSPlatform("windows")]
        public WindowsIdentity WindowsIdentity {
            get {
                if (_identity == null) {
                    _identity = WindowsIdentity.GetCurrent();
                }

                return _identity;
            }
        }

        [SupportedOSPlatform("windows")]
        public WindowsPrincipal WindowsPrincipal {
            get {
                if (_principal == null) {
                    _principal = new WindowsPrincipal(WindowsIdentity);
                }

                return _principal;
            }
        }

        [SupportedOSPlatform("windows")]
        public bool IsAdmin {
            get { return WindowsPrincipal.IsInRole(WindowsBuiltInRole.Administrator); }
        }

        internal Stack<EnvironmentFrame> EnvironmentStack {
            get {
                if (_environment == null) {
                    _environment = new Stack<EnvironmentFrame>();
                }

                return _environment;
            }
        }
    }
}
