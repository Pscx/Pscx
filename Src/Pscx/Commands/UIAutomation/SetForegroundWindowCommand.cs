//---------------------------------------------------------------------
// Author: Alex K. Angelopoulos
//
// Description: Sets the foreground window (e.g., top in Z-order).
//
// Why: Because some applications and out-of-process servers cause focus
//      loss; for example, IE will always grab the foreground.
// Creation Date: 2006-09-25
//---------------------------------------------------------------------
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Management.Automation;

namespace Pscx.Commands.UIAutomation
{
    [Cmdlet(VerbsCommon.Set, "ForegroundWindow")]
    [Description("Given an hWnd or window handle, brings that window to the foreground. Useful for restoring a window to uppermost after an application which seizes the foreground is invoked. See also Get-ForegroundWindow")]
    [RelatedLink(typeof(GetForegroundWindowCommand))]
    public class SetForegroundWindowCommand : Cmdlet
    {
        private IntPtr _handle = Process.GetCurrentProcess().MainWindowHandle;

        [Parameter(Position = 0, 
                   HelpMessage = "handle for the window to be set as the foreground window. If not specified, this defaults to the main window of the current process.")]
        public IntPtr Handle
        {
            get { return _handle; }
            set { _handle = value; }
        }

        protected override void EndProcessing()
        {
            if (SetForegroundWindow(_handle) == 0)
            {
                WriteWarning("SetForegroundWindow did not successfully set the foreground window.");
            }
        }

        [System.Runtime.InteropServices.DllImport("user32")]
        static extern Int32 SetForegroundWindow(IntPtr WindowHandle);
    }
}
