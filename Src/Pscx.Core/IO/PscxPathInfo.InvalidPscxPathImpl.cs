﻿//---------------------------------------------------------------------
// Author: Oisin Grehan
//
// Description: PscxPathInfo implementation for common resolved/literal
//              paths instance.
//
// Creation Date: September 12, 2007
//---------------------------------------------------------------------


namespace Pscx.Core.IO {
    partial class PscxPathInfo {
        /// <summary>
        /// Represents an invalid path in PowerShell.
        /// </summary>
        private class InvalidPscxPathImpl : PscxPathInfo {
            internal InvalidPscxPathImpl(string invalidPath) {
                _sourcePath = invalidPath;
            }

            public override string ToString() {
                return _sourcePath;
            }
        }
    }
}
