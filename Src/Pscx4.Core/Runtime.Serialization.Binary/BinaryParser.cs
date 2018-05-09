//---------------------------------------------------------------------
// Authors: jachymko
//
// Description: Binary structure reader.
//
// Creation Date: Dec 24, 2006
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Pscx.Runtime.Serialization.Binary
{
    public class BinaryParser : BinaryReader, IEnumerable<byte>
    {
        readonly long origin;

        public BinaryParser(Stream stream)
            : this(stream, 0)
        {
        }
        public BinaryParser(Stream stream, long origin)
            : base(stream)
        {
            this.origin = origin;
        }

        public long Position
        {
            get { return origin + BaseStream.Position; }
        }

        public void Align(int alignment)
        {
            if (alignment < 2) return;

            while ((Position % alignment) > 0) ReadByte();
        }

        public void SkipTo(uint offset)
        {
            BaseStream.Seek(origin + offset, SeekOrigin.Begin);
        }

        public byte PeekByte()
        {
            int b = BaseStream.ReadByte();
            if(b == -1)
            {
                throw new InvalidOperationException();
            }

            BaseStream.Seek(-1, SeekOrigin.Current);
            return (byte)(b);
        }

        public string ReadStringAsciiZ(int fixedSize)
        {
            byte[] buffer = ReadBytes(fixedSize);
            int len = 0;

            for (len = 0; len < fixedSize; len++)
            {
                if (buffer[len] == 0) break;
            }

            if (len > 0)
            {
                return Encoding.ASCII.GetString(buffer, 0, len);
            }

            return string.Empty;
        }

        public string ReadStringAsciiZ()
        {
            return ReadStringZ(Encoding.ASCII);
        }

        public String ReadStringZ(Encoding encoding)
        {
            using (var ms = new MemoryStream())
            {
                Byte lastByte = 0;

                do
                {
                    lastByte = ReadByte();
                    ms.WriteByte(lastByte);
                } 
                while ((lastByte > 0) && (BaseStream.Position < BaseStream.Length));

                return encoding.GetString(ms.GetBuffer(), 0, (Int32)(ms.Length - 1));
            }
        }

        public T ReadRecord<T>() where T : class, new()
        {
            return (T)(new RecordParser(typeof(T)).Parse(this));
        }

        public static T GetAttribute<T>(MemberInfo mi) where T : Attribute
        {
            object[] attrs = mi.GetCustomAttributes(typeof(T), true);
            if ((attrs == null) || (attrs.Length == 0))
            {
                return null;
            }

            return attrs[0] as T;
        }

        public static int BitCount(ulong n)
        {
            n = (n & 0x5555555555555555) + ((n >> 1) & 0x5555555555555555);
            n = (n & 0x3333333333333333) + ((n >> 2) & 0x3333333333333333);
            n = (n & 0x0F0F0F0F0F0F0F0F) + ((n >> 4) & 0x0F0F0F0F0F0F0F0F);

            return (int)(n % 0xFF);
        }

        public static int BitCount(uint n)
        {
            int count = 0;

            while (n > 0)
            {
                count++;
                n &= (n - 1);
            }

            return count;
        }

        #region IEnumerable<byte> Members

        public IEnumerator<byte> GetEnumerator()
        {
            while (true)
            {
                yield return ReadByte();
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion

    }
}
