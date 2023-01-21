//---------------------------------------------------------------------
// Author: jachymko
//
// Description: Adjust Token Privileges.
//
// Creation Date: Dec 19, 2006
//---------------------------------------------------------------------

using Pscx.Win.Interop.Security.Privileges;
using System.ComponentModel;
using System.Management.Automation;
using System.Runtime.Versioning;
using System.Security.Principal;

namespace Pscx.Win.Commands.Security {
    [Cmdlet(VerbsCommon.Set, PscxWinNouns.Privilege),
     Description("Adjusts privileges held by the session.")]
    [SupportedOSPlatform("windows")]
    public class SetPrivilegeCommand : WindowsIdentityCommandBase {
        [Parameter(Mandatory = true, Position = 0, HelpMessage = "The privileges to modify.")]
        public TokenPrivilegeCollection Privileges { get; set; }

        protected override void ProcessIdentity(WindowsIdentity identity) {
            using (var hToken = new SafeTokenHandle(identity.Token, false)) {
                Utils.AdjustTokenPrivileges(hToken, Privileges);
            }
        }
    }
}
