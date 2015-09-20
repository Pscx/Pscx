//---------------------------------------------------------------------
// Author: Keith Hill, jachymko
//
// Description: Structure for Encoding byte and string parameters.
//
// Creation Date: Dec 25, 2006
//---------------------------------------------------------------------
using System;
using System.Text;

namespace Pscx
{
    [Serializable]
    public struct EncodingParameter
    {
        private string _encodingName;

        public EncodingParameter(string name)
        {
            _encodingName = name;
        }

        public bool IsPresent
        {
            get { return !string.IsNullOrEmpty(_encodingName); }
        }

        public bool AsBytes
        {
            get 
            {
                return _encodingName.Equals("byte", StringComparison.OrdinalIgnoreCase);  
            }
        }

        public override string ToString()
        {
            return _encodingName;
        }

        public Encoding ToEncoding()
        {
            if (this.AsBytes)
            {
                return null;
            }
            return EncodingFromName(_encodingName);
        }

        internal static Encoding EncodingFromName(string encodingName)
        {
            if (string.IsNullOrEmpty(encodingName))
            {
                return UnicodeNoBom.UTF8;
            }
            else if (OrdinalIgnoreCaseEquals(encodingName, "unicode"))
            {
                return UnicodeNoBom.UTF16;
            }
            else if (OrdinalIgnoreCaseEquals(encodingName, "bigendianunicode"))
            {
                return UnicodeNoBom.BigEndian;
            }
            else if (OrdinalIgnoreCaseEquals(encodingName, "utf7"))
            {
                return UnicodeNoBom.UTF7;
            }
            else if (OrdinalIgnoreCaseEquals(encodingName, "utf8"))
            {
                return UnicodeNoBom.UTF8;
            }
            else if (OrdinalIgnoreCaseEquals(encodingName, "utf32"))
            {
                return UnicodeNoBom.UTF32;
            }
            else if (OrdinalIgnoreCaseEquals(encodingName, "ascii"))
            {
                return Encoding.ASCII;
            }
            else if (OrdinalIgnoreCaseEquals(encodingName, "default"))
            {
                return Encoding.Default;
            }

            return null;
        }

        static bool OrdinalIgnoreCaseEquals(string x, string y)
        {
            return string.Equals(x, y, StringComparison.OrdinalIgnoreCase);
        }

        static class UnicodeNoBom
        {
            public static readonly Encoding BigEndian = new UnicodeEncoding(true, false);

            public static readonly Encoding UTF7 = new UTF7Encoding();
            public static readonly Encoding UTF8 = new UTF8Encoding(false);
            public static readonly Encoding UTF16 = new UnicodeEncoding(false, false);
            public static readonly Encoding UTF32 = new UTF32Encoding(false, false);
        }
    }
}
