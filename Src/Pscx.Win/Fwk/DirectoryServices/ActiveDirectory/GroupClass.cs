using Pscx.Win.Fwk.DirectoryServices.DirectoryEntryProperties;

namespace Pscx.Win.Fwk.DirectoryServices.ActiveDirectory
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
