//---------------------------------------------------------------------
// Authors: Oisin Grehan
//
// Description: Interfaces and Classes needed to support querying of
//              the Global Assembly Cache
//
// Creation Date: Jan 31, 2007
// 
//---------------------------------------------------------------------

using Pscx.Win.Interop.Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Versioning;

namespace Pscx.Win.Reflection {
    [SupportedOSPlatform("windows")]
    class AssemblyCacheSearcher : IEnumerable<AssemblyName>, IDisposable {
        private readonly AssemblyCacheEnumerator _enumerator;

        internal AssemblyCacheSearcher(IAssemblyEnum enumerator) {
            _enumerator = new AssemblyCacheEnumerator(enumerator);
        }

        public void Dispose() {
            _enumerator.Dispose();
        }

        public IEnumerator<AssemblyName> GetEnumerator() {
            return _enumerator;
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return _enumerator;
        }
    }
}
