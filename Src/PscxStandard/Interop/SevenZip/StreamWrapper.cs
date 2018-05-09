using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Pscx.Interop.SevenZip
{
    //public class StreamWrapper : IDisposable
    //{
    //    protected Stream BaseStream;

    //    protected StreamWrapper(Stream baseStream)
    //    {
    //        BaseStream = baseStream;
    //    }

    //    public void Dispose()
    //    {
    //        BaseStream.Close();
    //    }

    //    public virtual void Seek(long offset, uint seekOrigin, IntPtr newPosition)
    //    {
    //        long Position = (uint)BaseStream.Seek(offset, (SeekOrigin)seekOrigin);
    //        if (newPosition != IntPtr.Zero)
    //            Marshal.WriteInt64(newPosition, Position);
    //    }
    //}

    public class StreamWrapper : IDisposable
    {
        // Fields
        protected Stream _BaseStream;

        // Methods
        protected StreamWrapper(Stream baseStream)
        {
            this._BaseStream = baseStream;
        }

        public virtual void Dispose()
        {
            this._BaseStream.Close();
        }

        public virtual void Seek(long offset, uint seekOrigin, IntPtr newPosition)
        {
            long Position = (uint)this._BaseStream.Seek(offset, (SeekOrigin)seekOrigin);
            if (newPosition != IntPtr.Zero)
            {
                Marshal.WriteInt64(newPosition, Position);
            }
        }

        // Properties
        public Stream BaseStream
        {
            get
            {
                return this._BaseStream;
            }
        }
    }
}