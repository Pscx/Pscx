//---------------------------------------------------------------------
// Author: jachymko
//
// Description: Class describing a simple, one-value entry property.
//
// Creation Date: 20 Feb 2007
//---------------------------------------------------------------------
using System;

using Pscx.Interop;

namespace Pscx.DirectoryServices
{
    public class SimpleDirectoryEntryProperty : DirectoryEntryProperty
    {
        private readonly string _attributeName;

        public SimpleDirectoryEntryProperty(string name, string attribName, DirectoryEntryPropertyAccess access)
            : base(name, access)
        {
            _attributeName = attribName;
        }

        protected override object OnGetValue()
        {
            try
            {
                object value = Entry.InvokeGet(_attributeName);

                IADsLargeInteger largeInteger = value as IADsLargeInteger;

                if (largeInteger != null)
                {
                    return DirectoryUtils.IADsLargeIntegerToDateTime(largeInteger);
                }

                return value;
            }
            catch (Exception exc)
            {
                return exc;
            }
        }

        protected override void OnSetValue(object value)
        {
            Entry.InvokeSet(_attributeName, value);
        }

        protected string AttributeName
        {
            get { return _attributeName; }
        }
    }
}
