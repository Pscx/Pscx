//---------------------------------------------------------------------
// Author: jachymko
//
// Description: Base class for WindowsIdentity-related commands.
//
// Creation Date: Dec 19, 2006
//---------------------------------------------------------------------
using System;
using System.Management.Automation;
using System.Security.Principal;

namespace Pscx.Commands.Security
{
    public abstract class WindowsIdentityCommandBase : PSCmdlet
    {
        WindowsIdentity userIdentity;

        [Parameter(Mandatory = false, Position = 1, ValueFromPipeline = true,
                   HelpMessage="The identity to act upon.")]
        public WindowsIdentity Identity
        {
            get { return userIdentity; }
            set { userIdentity = value; }
        }

        protected abstract void ProcessIdentity(WindowsIdentity identity);

        protected override void ProcessRecord()
        {
            if (userIdentity == null)
            {
                using(WindowsIdentity wi = WindowsIdentity.GetCurrent())
                {
                    ProcessIdentity(wi);
                }
            }
            else
            {
                ProcessIdentity(userIdentity);
            }   
        }
    }
}
