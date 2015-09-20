using System;
using System.Collections;
using System.Collections.Generic;

namespace Pscx.DirectoryServices.ActiveDirectory
{
    class GroupClass : DirectoryEntryType
    {
        public GroupClass()
            : base("group")
        {
        }

        protected override void OnCreateProperties()
        {
            AddProperty(new DNListDirectoryEntryProperty("Member", "member", DirectoryEntryPropertyAccess.ReadWrite));
        }
    }
}
