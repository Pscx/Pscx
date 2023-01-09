using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Pscx.Win.Interop
{
    // it would nearly be easier to write a wrapper around a powershell runspace
    // to do this kind of interop for us, but this class suffices for the moment.
    [SupportedOSPlatform("windows")]
    public class SimpleComWrapper : IDisposable
    {
        private object _comObject;
        private bool _isDisposed;
        private const BindingFlags _flags = BindingFlags.Instance | BindingFlags.Public;
        private readonly Type _comType;

        public SimpleComWrapper(Type comType)
        {
            _comType = comType;
            _comObject = Activator.CreateInstance(comType, true);
        }

        public object ComInstance
        {
            get
            {
                EnsureNotDisposed();
                return _comObject;
            }
        }

        public T GetPropertyValue<T>(string name)
        {
            EnsureNotDisposed();

            return (T) _comType.InvokeMember(
                           name,
                           _flags | BindingFlags.GetProperty, null,
                           this.ComInstance, null);
        }

        private void EnsureNotDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException("ComWrapper: " + _comObject);
            }
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                Marshal.ReleaseComObject(_comObject);
                _isDisposed = true;
                _comObject = null;
            }
        }
    }
}