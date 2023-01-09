//---------------------------------------------------------------------
// Authors: jachymko
//
// Description: Class which describes a PE file.
//
// Creation Date: Dec 24, 2006
//---------------------------------------------------------------------
using Pscx.Runtime.Serialization.Binary;
using System.Collections.Generic;
using System.IO;

namespace Pscx.Reflection
{
    public class PortableExecutableInfo
    {
        readonly string imagePath;

        readonly DosHeader dosHeader;
        readonly CoffHeader coffHeader;
        readonly PEHeader peHeader;
        readonly List<PESection> sections;

        public PortableExecutableInfo(string path)
        {
            this.imagePath = path;
        
            using(Stream stream = OpenImage())
            {
                using (BinaryParser br = new BinaryParser(stream))
                {
                    dosHeader = DosHeader.Parse(br);

                    br.SkipTo(dosHeader.CoffHeaderOffset);

                    coffHeader = br.ReadRecord<CoffHeader>();
                    peHeader = PEHeader.Parse(br);

                    sections = new List<PESection>();
                    for (int i = 0; i < coffHeader.NumberOfSections; i++)
                    {
                        sections.Add(br.ReadRecord<PESection>());
                    }
                }
            }
        }

        public string Path
        {
            get { return imagePath; }
        }

        public DosHeader DosHeader
        {
            get { return dosHeader; }
        }

        public CoffHeader CoffHeader
        {
            get { return coffHeader; }
        }

        public PEHeader PEHeader
        {
            get { return peHeader; }
        }

        public IList<PESection> Sections
        {
            get { return sections; }
        }

        protected PESection FindSectionByRva(uint rva)
        {
            for (int i = 0; i < sections.Count; i++)
            {
                uint sectionStart = sections[i].VirtualAdress;
                uint sectionEnd = sectionStart + sections[i].VirtualSize;

                if (rva >= sectionStart && rva < sectionEnd)
                {
                    return sections[i];
                }
            }

            return null;
        }

        protected BinaryParser OpenDirectory(PEDataDirectory dir)
        {
            PESection section = FindSectionByRva(dir.VirtualAddress);
            if (section == null)
            {
                InvalidPEFileException.ThrowInvalidRva();
            }

            uint index = (section.PointerToRawData + (dir.VirtualAddress - section.VirtualAdress));
            uint count = (dir.Size);

            Stream imageStream = OpenImage(index);

            return new BinaryParser(imageStream, index);
        }
 
        protected BinaryParser OpenSection(PESection section)
        {
            PscxArgumentException.ThrowIfIsNull(section);

            uint index = (section.PointerToRawData);
            uint count = (section.SizeOfRawData);

            return new BinaryParser(OpenImage(index));
        }

        protected Stream OpenImage()
        {
            return File.OpenRead(imagePath);
        }
        protected Stream OpenImage(long position)
        {
            Stream s = OpenImage();
            s.Position = position;

            return s;
        }

    }
}