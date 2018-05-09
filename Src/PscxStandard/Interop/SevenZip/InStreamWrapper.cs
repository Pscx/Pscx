using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Pscx.Interop.SevenZip
{
    //public class InStreamWrapper : StreamWrapper, ISequentialInStream, IInStream
    //{
    //    public InStreamWrapper(Stream baseStream) : base(baseStream) { }

    //    public uint Read(byte[] data, uint size)
    //    {
    //        return (uint)BaseStream.Read(data, 0, (int)size);
    //    }

    //    #region Implementation of ISequentialInStream

    //    uint ISequentialInStream.Read(IntPtr data, uint size)
    //    {
    //        return Read(data, size);
    //    }

    //    #endregion

    //    #region Implementation of IInStream

    //    uint IInStream.Read(IntPtr data, uint size)
    //    {
    //        return Read(data, size);
    //    }

    //    #endregion
    //}

    public class InStreamWrapper : StreamWrapper, ISequentialInStream, IInStream
    {
        // Methods
        public InStreamWrapper(Stream baseStream)
            : base(baseStream)
        {
        }

        public virtual uint Read(IntPtr data, uint size)
        {
            var Stream = base.BaseStream as FileStream;

            if (Stream != null)
            {
                uint Result;
                NativeMethods.ReadFile(Stream.SafeFileHandle, data, size, out Result, IntPtr.Zero);
                return Result;
            }
            
            var Buffer = new byte[Math.Min(0x4000, size)];
            int Readed = base._BaseStream.Read(Buffer, 0, Buffer.Length);
            
            Marshal.Copy(Buffer, 0, data, Readed);

            return (uint)Readed;
        }
    }
}

