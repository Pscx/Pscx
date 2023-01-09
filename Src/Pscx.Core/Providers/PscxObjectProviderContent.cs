using System;
using System.Collections;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Provider;

namespace Pscx.Providers
{
    internal sealed class PscxObjectProviderContent : IContentReader, IContentWriter
    {
        private readonly PSObject _object;
        private readonly String _path;

        private bool _contentRead;

        public PscxObjectProviderContent(PSObject obj, String path)
        {
            _object = obj;
            _path = path;
        }

        public void Close()
        {
        }

        public void Dispose()
        {
        }

        public void Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public IList Read(long readCount)
        {
            PSPropertyInfo info = _object.Properties[_path];
            IList result = null;

            if (!_contentRead && info != null)
            {
                Object value = info.Value;
                result = value as IList;

                if (result == null)
                {
                    result = new object[] { value };
                }

                _contentRead = true;
            }

            return result;
        }

        public IList Write(IList content)
        {
            object value = content;

            if (content.Count == 1)
            {
                value = content[0];
            }

            PSPropertyInfo info = _object.Properties[_path];

            if (info != null)
            {
                info.Value = value;
            }
            else
            {
                _object.Properties.Add(new PSNoteProperty(_path, value));
            }
            
            return content;
        }
    }
}
