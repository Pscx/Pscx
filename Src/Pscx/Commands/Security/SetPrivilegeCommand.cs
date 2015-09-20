//---------------------------------------------------------------------
// Author: jachymko
//
// Description: Adjust Token Privileges.
//
// Creation Date: Dec 19, 2006
//---------------------------------------------------------------------
using System.ComponentModel;
using System.Management.Automation;
using System.Security.Principal;
using Pscx.Interop;

namespace Pscx.Commands.Security
{
    [Cmdlet(VerbsCommon.Set, PscxNouns.Privilege)]
    [Description("Adjusts privileges held by the session.")]
    public class SetPrivilegeCommand : WindowsIdentityCommandBase
    {
        [Parameter(Mandatory = true, Position = 0, HelpMessage = "The privileges to modify.")]
        public TokenPrivilegeCollection Privileges { get; set; }

        protected override void ProcessIdentity(WindowsIdentity identity)
        {
            using (var hToken = new SafeTokenHandle(identity.Token, false))
            {
                Utils.AdjustTokenPrivileges(hToken, Privileges);
            }
        }
    }
}
