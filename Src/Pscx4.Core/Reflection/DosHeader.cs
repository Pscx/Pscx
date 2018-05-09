//---------------------------------------------------------------------
// Authors: jachymko
//
// Description: Class which describes a DOS header.
//
// Creation Date: Dec 24, 2006
//---------------------------------------------------------------------

using System;
using System.IO;

namespace Pscx.Reflection {

    public sealed class DosHeader {
        const ushort DosSignature = 0x5a4d; // MZ

        public ushort BytesOnLastPage;
        public ushort PageCount;
        public ushort RelocationCount;
        public ushort HeaderSize;
        public ushort MinExtraParagraphs;
        public ushort MaxExtraParagraphs;
        public ushort InitialSS;
        public ushort InitialSP;
        public ushort Checksum;
        public ushort InitialIP;
        public ushort InitialCS;
        public ushort RelocationTableOffset;
        public ushort OverlayNumber;
        public ushort OemID;
        public ushort OemInfo;
        public uint CoffHeaderOffset;

        public static DosHeader Parse(BinaryReader br) {
            PscxArgumentException.ThrowIfIsNull(br);

            ushort signature = br.ReadUInt16();
            if (DosSignature != signature) {
                InvalidPEFileException.ThrowInvalidDosHeader();
            }

            DosHeader hdr = new DosHeader();

            hdr.BytesOnLastPage = br.ReadUInt16();
            hdr.PageCount = br.ReadUInt16();
            hdr.RelocationCount = br.ReadUInt16();
            hdr.HeaderSize = br.ReadUInt16();
            hdr.MinExtraParagraphs = br.ReadUInt16();
            hdr.MaxExtraParagraphs = br.ReadUInt16();
            hdr.InitialSS = br.ReadUInt16();
            hdr.InitialSP = br.ReadUInt16();
            hdr.Checksum = br.ReadUInt16();
            hdr.InitialIP = br.ReadUInt16();
            hdr.InitialCS = br.ReadUInt16();
            hdr.RelocationTableOffset = br.ReadUInt16();
            hdr.OverlayNumber = br.ReadUInt16();

            // reserved words
            for (int i = 0; i < 4; i++) br.ReadUInt16();

            hdr.OemID = br.ReadUInt16();
            hdr.OemInfo = br.ReadUInt16();

            // reserved words
            for (int i = 0; i < 10; i++) br.ReadUInt16();

            hdr.CoffHeaderOffset = br.ReadUInt32();

            return hdr;
        }

    }
}
