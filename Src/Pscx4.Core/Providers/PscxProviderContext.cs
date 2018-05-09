//---------------------------------------------------------------------
// Author: jachymko
//
// Description: Holds current provider object instance.
//
// Creation Date: Feb 16, 2007
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Management.Automation.Provider;

namespace Pscx.Providers
{
    public static class PscxThreadContext
    {
        [ThreadStatic]
        private static Dictionary<object, object> _context;

        public static object Current(object key)
        {
            if (_context == null)
            {
                _context = new Dictionary<object, object>();
            }

            if (_context.ContainsKey(key))
            {
                return _context[key];
            }

            return null;
        }

        public static Cookie Enter(object key, object value)
        {
            PscxArgumentException.ThrowIfIsNull(key);

            Cookie cookie = new Cookie(key, Current(key));

            _context[key] = value;
            
            return cookie;
        }

        public struct Cookie : IDisposable
        {
            private readonly object _key;
            private readonly object _previous;

            internal Cookie(object key, object previous)
            {
                _key = key;
                _previous = previous;
            }

            public void Dispose()
            {
                _context[_key] = _previous;
            }
        }
    }

    public static class PscxProviderContext<TProvider> where TProvider : CmdletProvider
    {
        public static TProvider Current
        {
            get { return (TProvider)PscxThreadContext.Current(typeof(TProvider)); }
        }

        public static PscxThreadContext.Cookie Enter(TProvider provider)
        {
            PscxArgumentException.ThrowIfIsNull(provider);

            return PscxThreadContext.Enter(typeof(TProvider), provider);
        }
    }
}
