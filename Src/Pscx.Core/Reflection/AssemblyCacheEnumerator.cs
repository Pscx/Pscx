//---------------------------------------------------------------------
// Authors: Oisin Grehan, jachymko
//
// Description: Interfaces and Classes needed to support querying of
//              the Global Assembly Cache
//
// Creation Date: Jan 31, 2007
// 
//---------------------------------------------------------------------

// Portions from: http://dotnetjunkies.com/WebLog/debasish/archive/2006/12/03/164789.aspx
// modified by Oisin Grehan:
//      added ASM_CACHE_FLAGS
//      added generic enumerator
//      integrated KB317540
 
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

using Pscx.Interop;

namespace Pscx.Reflection
{    
    internal class AssemblyCacheEnumerator : IEnumerator<AssemblyName>, IDisposable
    {
        private IAssemblyEnum _enumerator;
        private AssemblyName _current;

		public AssemblyCacheEnumerator(IAssemblyEnum enumerator)
        {
            _enumerator = enumerator;
        }

        public bool MoveNext()
        {
            EnforceNotDisposed();

            IAssemblyName native;

            if (_enumerator.GetNextAssembly(IntPtr.Zero, out native, 0) == 0)
            {
                try
                {
                    _current = AssemblyNameConvertor.ToAssemblyName(native);
                }
                finally
                {
                    Marshal.ReleaseComObject(native);
                }
            }
            else
            {
                _current = null;
            }

            return (_current != null);
        }

        public void Reset()
        {
            EnforceNotDisposed();

            _enumerator.Reset();
        }

        // Properties
        public AssemblyName Current
        {
            get { return _current; }
        }

        public void Dispose()
        {
            Marshal.ReleaseComObject(_enumerator);

            _enumerator = null;
            _current = null;
        }

        private void EnforceNotDisposed()
        {
            if (_enumerator == null)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }

        object IEnumerator.Current
        {
            get { return Current; }
        }
    }
}
