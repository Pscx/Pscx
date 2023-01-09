//---------------------------------------------------------------------
// Author: jachymko
//
// Description: Base class for streams stream wrappers.
//
// Creation Date: Jan 24, 2007
//---------------------------------------------------------------------

using System;
using System.IO;

namespace Pscx.Core.IO
{
    public abstract class StreamDecorator : Stream
    {
        private Stream _inner;

        protected StreamDecorator()
        {
        }

        protected StreamDecorator(Stream inner)
        {
            _inner = inner;
        }

        protected Stream InnerStream
        {
            get { return _inner; }
            set { _inner = value; }
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return _inner.BeginRead(buffer, offset, count, callback, state);
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return _inner.BeginWrite(buffer, offset, count, callback, state);
        }

        public override bool CanRead
        {
            get { return _inner.CanRead; }
        }

        public override bool CanSeek
        {
            get { return _inner.CanSeek; }
        }

        public override bool CanTimeout
        {
            get { return _inner.CanTimeout; }
        }

        public override bool CanWrite
        {
            get { return _inner.CanWrite; }
        }

        public override void Close()
        {
            _inner.Close();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _inner.Dispose();
            }

            base.Dispose(disposing);
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            return _inner.EndRead(asyncResult);
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            _inner.EndWrite(asyncResult);
        }

        public override void Flush()
        {
            _inner.Flush();
        }

        public override long Length
        {
            get { return _inner.Length; }
        }

        public override long Position
        {
            get { return _inner.Position; }
            set { _inner.Position = value; }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _inner.Read(buffer, offset, count);
        }

        public override int ReadByte()
        {
            return _inner.ReadByte();
        }

        public override int ReadTimeout
        {
            get { return _inner.ReadTimeout; }
            set { _inner.ReadTimeout = value; }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _inner.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _inner.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _inner.Write(buffer, offset, count);
        }

        public override void WriteByte(byte value)
        {
            _inner.WriteByte(value);
        }

        public override int WriteTimeout
        {
            get { return _inner.WriteTimeout; }
            set { _inner.WriteTimeout = value; }
        }
    }
}
