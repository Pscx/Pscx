//---------------------------------------------------------------------
// Authors: jachymko
//
// Description: Class which describes a Portable Executable header.
//
// Creation Date: Dec 24, 2006
//---------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;

using Pscx.Runtime.Serialization.Binary;

namespace Pscx.Reflection
{
    public sealed class PEHeader
    {
        public PEHeaderType Type;

        public Version LinkerVersion;
        public Version OperatingSystemVersion;
        public Version ImageVersion;
        public Version SubsystemVersion;
        public uint SizeOfCode;
        public uint SizeOfInitializedData;
        public uint SizeOfUninitializedData;
        public uint AddressOfEntryPoint;
        public uint BaseOfCode;
        public uint BaseOfData;

        public ulong ImageBase;
        public uint SectionAlignment;
        public uint FileAlignment;
        public uint Win32VersionValue;
        public uint SizeOfImage;
        public uint SizeOfHeaders;
        public uint Checksum;
        public PESubsystem Subsystem;
        public PEDllCharacteristics DllCharacteristics;
        public ulong SizeOfStackReserve;
        public ulong SizeOfStackCommit;
        public ulong SizeOfHeapReserve;
        public ulong SizeOfHeapCommit;
        public uint LoaderFlags;

        public PEDataDirectory[] DataDirectories;

        public PEHeader()
        {
        }

        public PEDataDirectory GetDataDirectory(DataDirectory index)
        {
            PscxArgumentException.ThrowIfIsNotDefined(index);

            return DataDirectories[(int)(index)];
        }

        public static PEHeader Parse(BinaryParser br)
        {
            PEHeaderType signature = (PEHeaderType)br.ReadUInt16();
            if (!Enum.IsDefined(typeof(PEHeaderType), signature))
            {
                InvalidPEFileException.ThrowInvalidPEHeader();
            }

            PEHeader hdr = new PEHeader();

            hdr.Type = signature;
            hdr.LinkerVersion = new Version(br.ReadByte(), br.ReadByte());
            hdr.SizeOfCode = br.ReadUInt32();
            hdr.SizeOfInitializedData = br.ReadUInt32();
            hdr.SizeOfUninitializedData = br.ReadUInt32();
            hdr.AddressOfEntryPoint = br.ReadUInt32();
            hdr.BaseOfCode = br.ReadUInt32();

            if (signature == PEHeaderType.PE64)
            {
                hdr.ImageBase = br.ReadUInt64();
            }
            else
            {
                hdr.BaseOfData = br.ReadUInt32();
                hdr.ImageBase = br.ReadUInt32();
            }

            hdr.SectionAlignment = br.ReadUInt32();
            hdr.FileAlignment = br.ReadUInt32();
            hdr.OperatingSystemVersion = new Version(br.ReadUInt16(), br.ReadUInt16());
            hdr.ImageVersion = new Version(br.ReadUInt16(), br.ReadUInt16());
            hdr.SubsystemVersion = new Version(br.ReadUInt16(), br.ReadUInt16());
            hdr.Win32VersionValue = br.ReadUInt32();
            hdr.SizeOfImage = br.ReadUInt32();
            hdr.SizeOfHeaders = br.ReadUInt32();
            hdr.Checksum = br.ReadUInt32();
            hdr.Subsystem = (PESubsystem)br.ReadUInt16();
            hdr.DllCharacteristics = (PEDllCharacteristics)br.ReadUInt16();

            if (signature == PEHeaderType.PE64)
            {
                hdr.SizeOfStackReserve = br.ReadUInt64();
                hdr.SizeOfStackCommit = br.ReadUInt64();
                hdr.SizeOfHeapReserve = br.ReadUInt64();
                hdr.SizeOfHeapCommit = br.ReadUInt64();
            }
            else
            {
                hdr.SizeOfStackReserve = br.ReadUInt32();
                hdr.SizeOfStackCommit = br.ReadUInt32();
                hdr.SizeOfHeapReserve = br.ReadUInt32();
                hdr.SizeOfHeapCommit = br.ReadUInt32();
            }

            hdr.LoaderFlags = br.ReadUInt32();

            hdr.DataDirectories = new PEDataDirectory[br.ReadUInt32()];
            for (int i = 0; i < hdr.DataDirectories.Length; i++)
            {
                hdr.DataDirectories[i] = br.ReadRecord<PEDataDirectory>();
            }

            return hdr;
        }
    }

    public enum DataDirectory : int 
    {
        Export,
        Import,
        Resource,
        Exception,
        Security,
        BaseRelocationTable,
        Debug,
        ArchitectureData,
        GlobalPointer,
        ThreadLocalStorage,
        LoadConfiguration,
        BoundImport,
        ImportAddressTable,
        DelayLoadImport,
        CorHeader,
    }

    [BinaryRecord]
    public sealed class PEDataDirectory
    {
        public uint VirtualAddress;
        public uint Size;

        public PEDataDirectory()
        {
        }

        public override string ToString()
        {
            return string.Format("PEDataDirectory, RVA=0x{0:x}, Size=0x{1:x}",
                VirtualAddress, Size);
        }
    }

    public enum PEHeaderType : ushort
    {
        PE32 = 0x10b,
        PE64 = 0x20b,
        RomImage = 0x107,
    }

    public enum PESubsystem : ushort
    {
        Unknown = 0,
        Native = 1,
        Windows = 2,
        WindowsConsole = 3,
        OS2 = 5,
        Posix = 7,
        WindowsCE = 9,
        EfiApplication = 10,
        EfiBootServiceDriver = 11,
        EfiRuntimeDriver = 12,
        EfiRomImage = 13,
        Xbox = 14,
        WindowsBootApplication = 16,
    }

    public enum PEDllCharacteristics : ushort 
    {
        DynamicBase = 0x0040,
        ForceIntegrity = 0x0080,
        NXCompatible = 0x0100,
        NoIsolation = 0x0200,
        NoSeh = 0x0400,
        NoBind = 0x0800,
        WdmDriver = 0x2000,
        TerminalServicesAware = 0x8000,
    }
}
