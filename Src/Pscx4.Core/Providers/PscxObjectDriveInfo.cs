//---------------------------------------------------------------------
// Author: jachymko
//
// Description: Keeps the object for a PscxObjectProviderBase drive.
//
// Creation Date: Feb 14, 2008
//---------------------------------------------------------------------

using System;
using System.Management.Automation;

namespace Pscx.Providers
{
    public class PscxObjectDriveInfo : PSDriveInfo
    {
        public PscxObjectDriveInfo(string name, ProviderInfo provider, string description, PSObject obj)
            : base(name, provider, string.Empty, description, null)
        {
            DriveObject = obj;
        }

        public PSObject DriveObject
        {
            get;
            private set;
        }
    }
}
