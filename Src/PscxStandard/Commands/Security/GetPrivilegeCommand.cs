//---------------------------------------------------------------------
// Author: TonyDeSweet
// Modifications: Alex K. Angelopoulos, jachymko
//
// Description: Dump Token Privilege.
//
// Creation Date: Feb 21, 2006
//---------------------------------------------------------------------
using System.ComponentModel;
using System.Management.Automation;
using System.Security.Principal;
using Pscx.Interop;

namespace Pscx.Commands.Security
{
    [OutputType(typeof(TokenPrivilegeCollection))]
    [Cmdlet(VerbsCommon.Get, PscxNouns.Privilege)]
    [Description("Lists privileges held by the session and their current status.")]
    public class GetPrivilegeCommand : WindowsIdentityCommandBase
    {
        protected override void ProcessIdentity(WindowsIdentity identity)
        {
            WriteObject(new TokenPrivilegeCollection(identity));
        }
    }
}
