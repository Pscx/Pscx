//---------------------------------------------------------------------
// Author: jachymko
//
// Description: Dictionary of entry properties.
//
// Creation Date: 13 Feb 2007
//---------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Pscx.DirectoryServices
{
    public class DirectoryEntryPropertyDictionary : Dictionary<String, DirectoryEntryProperty>
    {
        public DirectoryEntryPropertyDictionary ()
            : base (StringComparer.OrdinalIgnoreCase)
	    {

	    }

        public void Add(DirectoryEntryProperty property)
        {
            this[property.Name] = property;
        }

        public void AddRange(IEnumerable<DirectoryEntryProperty> properties)
        {
            foreach (DirectoryEntryProperty prop in properties)
            {
                Add(prop);
            }
        }
    }
}
