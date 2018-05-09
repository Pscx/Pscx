//---------------------------------------------------------------------
// Author: jachymko
//
// Description: Common base for rich providers.
//
// Creation Date: Mar 13, 2007
//---------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using System.Management.Automation.Provider;

namespace Pscx.Providers
{
    public abstract class PscxNavigationCmdletProvider : NavigationCmdletProvider
    {
        public const char Slash = '/';
        public const char Backslash = '\\';

    }
}
