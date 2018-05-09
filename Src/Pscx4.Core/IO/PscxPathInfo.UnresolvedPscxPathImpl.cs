//---------------------------------------------------------------------
// Author: Oisin Grehan
//
// Description: PscxPathInfo implementation for common unresolved/literal
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
        /// Represents a literal/unresolved path in PowerShell.
        /// </summary>
        private class UnresolvedPscxPathImpl : PscxPathInfo
        {
            /// <summary>
            ///
            /// </summary>
            /// <param name="literalPath"></param>
            /// <param name="session"></param>
            internal UnresolvedPscxPathImpl(string literalPath, SessionState session)
            {
                PscxArgumentException.ThrowIfIsNullOrEmpty(literalPath);
                PscxArgumentException.ThrowIfIsNull(session);

                _providerPath = session.Path.GetUnresolvedProviderPathFromPSPath(literalPath, out _providerInfo, out _driveInfo);
                _isUnresolved = true;
                _sourcePath = literalPath;                
            }

            public override string ToString()
            {
                string path;
                if (this._driveInfo == null)
                {
                    path = GetProviderQualifiedPath(this._providerPath, this._providerInfo);
                }
                else
                {
                    path = GetDriveQualifiedPath(this._providerPath, this._driveInfo);
                }
                return path;
            }
        }
	}
}
