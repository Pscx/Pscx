//---------------------------------------------------------------------
// Author: Oisin Grehan
//
// Description: PscxPathInfo implementation for common resolved/literal
//              paths instance.
//
// Creation Date: September 12, 2007
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Text;

namespace Pscx.IO
{
    partial class PscxPathInfo
    {
        /// <summary>
        /// Represents a resolvable path in PowerShell.
        /// </summary>
        private class ResolvedPscxPathImpl : PscxPathInfo
        {
            internal ResolvedPscxPathImpl(PathInfo resolvedPath)
            {
                _pathInfo = resolvedPath;                
                _providerInfo = resolvedPath.Provider;
                _driveInfo = resolvedPath.Drive;
                _providerPath = resolvedPath.ProviderPath;
                _isUnresolved = false;
            }

            internal ResolvedPscxPathImpl(PathInfo resolvedPath, string sourcePath) :
                this(resolvedPath)
            {
                _sourcePath = sourcePath; // save original source path, e.g. "*.txt"
            }

            public override string ToString()
            {
                return _pathInfo.ToString();
            }
        }
    }
}
