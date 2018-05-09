//---------------------------------------------------------------------
// Authors: jachymko
//
// Description: Class which describes section of a PE file.
//
// Creation Date: Dec 24, 2006
//---------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Pscx.Reflection {

    using Runtime.Serialization.Binary;

    [BinaryRecord]
    public sealed class PESection {
        [BinaryField(BinaryFieldType.StringAsciiZ, FixedLength=8)]
        public string Name;
        public uint VirtualSize;
        public uint VirtualAdress;
        public uint SizeOfRawData;
        public uint PointerToRawData;
        public uint PointerToRelocations;
        public uint PointerToLineNumbers;
        public ushort NumberOfRelocations;
        public ushort NumberOfLineNumbers;
        public SectionFlags Flags;

        public PESection() {
        }

        public override string ToString() {
            return Name;
        }

    }

    [Flags]
    public enum SectionFlags : uint {
        TlsIndexScaled = 0x00000001,
        Code = 0x00000020,
        InitializedData = 0x00000040,
        UninitializedData = 0x00000080,
        DeferSpecExc = 0x00004000,
        NRelocOvfl = 0x01000000,
        Discardable = 0x02000000,
        NotCached = 0x04000000,
        NotPaged = 0x08000000,
        Shared = 0x10000000,
        Execute = 0x20000000,
        Read = 0x40000000,
        Write = 0x80000000,
    }
}
