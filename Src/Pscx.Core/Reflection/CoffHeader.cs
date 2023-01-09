//---------------------------------------------------------------------
// Authors: jachymko
//
// Description: Class which describes a COFF header.
//
// Creation Date: Dec 24, 2006
//---------------------------------------------------------------------
using System;

namespace Pscx.Reflection
{
    using Runtime.Serialization.Binary;

    [BinaryRecord]
    public sealed class CoffHeader
    {
        [MagicSignature(0x00004550u)] // PE00
        public uint Magic;

        public CoffMachine Machine;
        public ushort NumberOfSections;
        [BinaryField(BinaryFieldType.UnixTime)]
        public DateTime TimeDateStamp;
        public uint PointerToSymbolTable;
        public uint NumberOfSymbols;
        public ushort SizeOfOptionalHeader;
        public CoffFlags Characteristics;
    }

    public enum CoffMachine : ushort
    {
        Unknown = 0x0000,
        I386 = 0x014c,
        IA64 = 0x0200,
        Amd64 = 0x8664,
    }

    [Flags]
    public enum CoffFlags : ushort
    {
        RelocsStripped = 0x0001,
        ExecutableImage = 0x0002,
        LineNumsStripped = 0x0004,
        LocalSymsStripped = 0x0008,
        AggresiveWsTrim = 0x0010,
        LargeAddressAware = 0x0020,
        BytesReversedLow = 0x0080,
        Machine32Bit = 0x0100,
        DebugStripped = 0x0200,
        RemovableRunFromSwap = 0x0400,
        NetworkRunFromSwap = 0x0800,
        System = 0x1000,
        Dll = 0x2000,
        UniProcOnly = 0x4000,
        BytesReversedHi = 0x8000,
    }
}
