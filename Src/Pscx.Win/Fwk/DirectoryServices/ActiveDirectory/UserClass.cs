//---------------------------------------------------------------------
// Author: jachymko, Reinhard Lehrbaum
//
// Description: 
//
// Creation Date: 6 Feb 2007
//---------------------------------------------------------------------
using System.DirectoryServices;

namespace Pscx.Win.Fwk.DirectoryServices.ActiveDirectory {
    partial class UserClass : DirectoryEntryType {
        public UserClass()
            : base("user") {
        }

        protected override void OnCreateProperties() {
            AddReadOnlyProperty("BadPasswordTime", "badPasswordTime");
            AddReadOnlyProperty("BadPasswordCount", "badPwdCount");
            AddSimpleProperty("Department", "department");
            AddSimpleProperty("DisplayName", "displayName");
            AddSimpleProperty("FirstName", "givenName");
            AddReadOnlyProperty("LastLogon", "lastLogon");
            AddReadOnlyProperty("LastLogoff", "lastLogoff");
            AddReadOnlyProperty("LogonCount", "logonCount");
            AddSimpleProperty("Mail", "mail");
            AddDNProperty("Manager", "manager");
            AddSimpleProperty("OfficeName", "physicalDeliveryOfficeName");
            AddSetMethodProperty("Password", "SetPassword");
            AddReadOnlyProperty("PasswordLastSet", "pwdLastSet");
            AddReadOnlyProperty("SamAccountName", "sAMAccountName");
            AddSimpleProperty("Surname", "sn");
            AddSimpleProperty("Telephone", "telephoneNumber");
            AddSimpleProperty("Title", "title");
            AddReadOnlyProperty("UserPrincipalName", "userPrincipalName");

            AddUacProperty(UserAccountFlag.Disabled);
            AddUacProperty(UserAccountFlag.DontRequirePreauth);
            AddUacProperty(UserAccountFlag.LockedOut);
            AddUacProperty(UserAccountFlag.NotDelegated);
            AddUacProperty(UserAccountFlag.PasswordCantChange);
            AddUacProperty(UserAccountFlag.PasswordNeverExpires);
            AddUacProperty(UserAccountFlag.PasswordNotRequired);
            AddUacProperty(UserAccountFlag.PasswordEncryptedTextAllowed);
            AddUacProperty(UserAccountFlag.SmartcardRequired);
            AddUacProperty(UserAccountFlag.TrustedForDelegation);
            AddUacProperty(UserAccountFlag.TrustedToAuthenticateForDelegation);

            AddUacPropertyRO(UserAccountFlag.InterDomainTrustAccount);
            AddUacPropertyRO(UserAccountFlag.ServerTrustAccount);
            AddUacPropertyRO(UserAccountFlag.WorkstationTrustAccount);
        }

        public override DirectoryEntry NewItem(DirectoryEntry parent, string name) {
            DirectoryEntry entry = base.NewItem(parent, name);

            string domainName = ActiveDirectoryUtils.GetDomainDnsName(parent);

            entry.Properties["sAMAccountName"].Value = name;
            entry.Properties["userPrincipalName"].Value = name + '@' + domainName;

            return entry;
        }

        private void AddUacProperty(UserAccountFlag mask) {
            AddUacProperty(Core.Utils.GetEnumName(mask), mask);
        }

        private void AddUacPropertyRO(UserAccountFlag mask) {
            AddUacProperty(Core.Utils.GetEnumName(mask), mask, DirectoryEntryPropertyAccess.Read);
        }

        private void AddUacProperty(string name, UserAccountFlag mask) {
            AddUacProperty(name, mask, DirectoryEntryPropertyAccess.ReadWrite);
        }

        private void AddUacProperty(string name, UserAccountFlag mask, DirectoryEntryPropertyAccess access) {
            AddProperty(new UserAccountControlProperty(name, access, mask));
        }
    }
}
