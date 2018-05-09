//---------------------------------------------------------------------
// Author: jachymko
//
// Description: A class for creating a simple IDisposable object
//              from a delegate.
//
// Creation Date: Sep 17, 2007
//---------------------------------------------------------------------
using System;

namespace Pscx
{
    public sealed class GenericDisposable : IDisposable
    {
        private readonly Action _onDisposing;

        public GenericDisposable(Action onDisposing)
        {
            _onDisposing = onDisposing;
        }

        public void Dispose()
        {
            if (_onDisposing != null)
            {
                _onDisposing();
            }
        }
    }
}
