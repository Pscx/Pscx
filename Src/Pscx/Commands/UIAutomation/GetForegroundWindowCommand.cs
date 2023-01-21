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
using System.Management.Automation;

namespace Pscx.Commands.UIAutomation
{
    [Cmdlet(VerbsCommon.Get, PscxNouns.ForegroundWindow), 
     Description("Returns the hWnd or handle of the window in the foreground on the current desktop. See also Set-ForegroundWindow.")]
    [RelatedLink(typeof(SetForegroundWindowCommand))]
    public class GetForegroundWindowCommand : Cmdlet
    {
        protected override void BeginProcessing()
        {
            WriteObject(GetForegroundWindow());
        }

        [System.Runtime.InteropServices.DllImport("user32")]
        static extern IntPtr GetForegroundWindow();
    }
}
