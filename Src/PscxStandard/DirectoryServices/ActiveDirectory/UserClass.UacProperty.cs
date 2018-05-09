//---------------------------------------------------------------------
// Author: jachymko
//
// Description: Provides access to the userAccountControl-stored properties.
//
// Creation Date: Feb 12, 2007
//---------------------------------------------------------------------
using System;
using System.Management.Automation;

namespace Pscx.DirectoryServices.ActiveDirectory
{
    partial class UserClass
    {
        [Flags]
        enum UserAccountFlag
        {
            Script = 1,
            Disabled = 2,
            HomeDirRequired = 8,
            LockedOut = 16,
            PasswordNotRequired = 32,
            PasswordCantChange = 64,
            PasswordEncryptedTextAllowed = 128,
            TempDuplicateAccount = 256,
            NormalAccount = 512,
            InterDomainTrustAccount = 2048,
            WorkstationTrustAccount = 4096,
            ServerTrustAccount = 8192,
            PasswordNeverExpires = 65536,
            MnsLogonAccount = 131072,
            SmartcardRequired = 262144,
            TrustedForDelegation = 524288,
            NotDelegated = 1048576,
            UseDesKeyOnly = 2097152,
            DontRequirePreauth = 4194304,
            PasswordExpired = 8388608,
            TrustedToAuthenticateForDelegation = 16777216,
        }

        class UserAccountControlProperty : DirectoryEntryProperty
        {
            private UserAccountFlag _mask;

            public UserAccountControlProperty(string name, DirectoryEntryPropertyAccess access, UserAccountFlag mask)
                : base(name, access)
            {
                _mask = mask;
            }
            protected override object OnGetValue()
            {
                return (Value & _mask) == _mask;
            }

            protected override void OnSetValue(object enable)
            {
                if (LanguagePrimitives.IsTrue(enable))
                {
                    Value |= _mask;
                }
                else
                {
                    Value &= ~_mask;
                }
            }

            private UserAccountFlag Value
            {
                get { return (UserAccountFlag)Entry.InvokeGet("userAccountControl"); }
                set { Entry.InvokeSet("userAccountControl", value); }
            }
        }
    }
}