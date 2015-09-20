using System;
using System.Management.Automation;
using System.Security.Principal;

namespace Pscx.Commands.Security
{
    [OutputType(typeof(bool))]
    [
        Cmdlet(VerbsDiagnostic.Test, "UserGroupMembership",
        DefaultParameterSetName = PARAMSET_NAME)
    ]
    public class TestUserGroupMembershipCommand : WindowsIdentityCommandBase
    {
        private const string PARAMSET_NAME = "name";
        private const string PARAMSET_ID = "id";

        private readonly IdentityReferenceCollection _references = new IdentityReferenceCollection();

        [Parameter(Position = 0, Mandatory = true, ParameterSetName = PARAMSET_NAME)]
        [ValidateNotNull]        
        public string[] GroupName
        {
            get;
            set;
        }

        [Parameter(Position = 0, Mandatory = true, ParameterSetName = PARAMSET_ID)]
        [ValidateNotNull]
        public IdentityReference[] IdentityReference
        {
            get
            {
                var references = (IdentityReference[])Array.CreateInstance(
                    typeof(IdentityReference), _references.Count);
                _references.CopyTo(references, 0);

                return references;
            }
            set
            {
                foreach (var reference in value)
                {
                    _references.Add(reference);
                }
            }
        }

        protected override void ProcessIdentity(WindowsIdentity identity)
        {
            bool result = true;

            if (this.ParameterSetName == PARAMSET_NAME)
            {
                if (!TranslateGroupNames())
                {
                    result = false;
                }
            }

            if (result)
            {
                foreach (var reference in _references)
                {
                    // ReSharper disable PossibleNullReferenceException
                    if (!identity.Groups.Contains(reference))
                    // ReSharper restore PossibleNullReferenceException
                    {
                        result = false;
                        break;
                    }
                }
            }
            WriteObject(result);
        }

        private bool TranslateGroupNames()
        {
            bool success = true;

            foreach (string name in this.GroupName)
            {
                var account = new NTAccount(name);
                try
                {
                    var reference = account.Translate(typeof (SecurityIdentifier));
                    _references.Add(reference);
                }
                catch (IdentityNotMappedException)
                {
                    WriteWarning(String.Format("'{0}' not recognized as a local or domain security group.", name));
                    success = false;
                }                
            }
            
            return success;
        }
    }
}
