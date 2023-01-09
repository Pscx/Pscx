//---------------------------------------------------------------------
// Author: jachymko
//
// Description: Collection of entry types.
//
// Creation Date: 13 Feb 2007
//---------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.DirectoryServices;

namespace Pscx.Win.Fwk.DirectoryServices
{
    public class DirectoryEntryTypeCollection : ReadOnlyCollection<DirectoryEntryType>
    {
        private DirectoryEntryTypeCollection(List<DirectoryEntryType> list)
            : base (list)
        {
        }

        public DirectoryEntryType MostDerivedType
        {
            get
            {
                if (Count > 0)
                {
                    return this[Count - 1];
                }

                return null;
            }
        }

        public IEnumerable<DirectoryEntryType> Reversed
        {
            get
            {
                for (int i = Count - 1; i >= 0; i--)
                {
                    yield return this[i];
                }
            }
        }

        public static DirectoryEntryTypeCollection FromDirectoryEntry(DirectoryEntry entry)
        {
            List<DirectoryEntryType> types = new List<DirectoryEntryType>();

            foreach (string className in DirectoryUtils.GetPropertyValueAsStringArray(entry, "objectClass"))
            {
                DirectoryEntryType type = DirectoryEntryType.FromClassName(className);

                if (type != null)
                {
                    types.Add(type);
                }
            }

            return new DirectoryEntryTypeCollection(types);
        }
    }
}
