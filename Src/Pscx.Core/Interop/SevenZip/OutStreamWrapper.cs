using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Pscx.Interop.SevenZip
{
    public class OutStreamWrapper : StreamWrapper, ISequentialOutStream, IOutStream
    {
        public OutStreamWrapper(Stream baseStream) : base(baseStream) { }

        public int SetSize(long newSize)
        {
            BaseStream.SetLength(newSize);
            return 0;
        }

        public int Write(byte[] data, uint size, IntPtr processedSize)
        {
            BaseStream.Write(data, 0, (int)size);
            if (processedSize != IntPtr.Zero)
            {
                Marshal.WriteInt32(processedSize, (int) size);
            }
            return 0;
        }
    }
}