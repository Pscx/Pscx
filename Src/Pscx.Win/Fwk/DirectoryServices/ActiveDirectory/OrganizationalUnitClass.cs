//---------------------------------------------------------------------
// Author: jachymko, Reinhard Lehrbaum
//
// Description: 
//
// Creation Date: 6 Feb 2007
//---------------------------------------------------------------------
using System;

namespace Pscx.Win.Fwk.DirectoryServices.ActiveDirectory
{
    class OrganizationalUnitClass : DirectoryEntryType
    {
        public OrganizationalUnitClass()
            : base("organizationalUnit", true)
        {
        }

        protected override string NamePrefix
        {
            get { return "OU=";  }
        }
    }
}
