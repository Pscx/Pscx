//---------------------------------------------------------------------
// Author: TonyDeSweet
// Modifications: Alex K. Angelopoulos, jachymko
//
// Description: Dump Token Privilege.
//
// Creation Date: Feb 21, 2006
//---------------------------------------------------------------------

using Pscx.Win.Interop.Security.Privileges;
using System.ComponentModel;
using System.Management.Automation;
using System.Runtime.Versioning;
using System.Security.Principal;

namespace Pscx.Win.Commands.Security {
    [OutputType(typeof(TokenPrivilegeCollection))]
    [Cmdlet(VerbsCommon.Get, PscxWinNouns.Privilege),
     Description("Lists privileges held by the session and their current status.")]
    [SupportedOSPlatform("windows")]
    public class GetPrivilegeCommand : WindowsIdentityCommandBase {
        protected override void ProcessIdentity(WindowsIdentity identity) {
            WriteObject(new TokenPrivilegeCollection(identity));
        }
    }
}
