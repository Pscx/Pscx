//---------------------------------------------------------------------
// Author: jachymko
//
// Description: Interface for properties which, in fact, contain
//              distinguished name strings.
//
// Creation Date: 20 Feb 2007
//---------------------------------------------------------------------
using System;

namespace Pscx.DirectoryServices
{
    public interface IDistinguishedNameProperty
    {
    }

    public class DNListDirectoryEntryProperty : ListDirectoryEntryProperty, IDistinguishedNameProperty
    {
        public DNListDirectoryEntryProperty(string name, string attribute, DirectoryEntryPropertyAccess access)
            : base(name, attribute, access)
        {
        }

        protected override object OnGetValue()
        {
            return DirectoryUtils.GetPropertyValueAsStringArray(Entry, AttributeName);
        }
    }

    public class DNSimpleDirectoryEntryProperty : SimpleDirectoryEntryProperty, IDistinguishedNameProperty
    {
        public DNSimpleDirectoryEntryProperty(string name, string attribute, DirectoryEntryPropertyAccess access)
            : base(name, attribute, access)
        {
        }

        protected override object OnGetValue()
        {
            return DirectoryUtils.GetPropertyValueAsString(Entry, AttributeName);
        }
    }

}
