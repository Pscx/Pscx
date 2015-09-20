//---------------------------------------------------------------------
// Author: Keith Hill
//
// Description: Class to represent info returned by Get-Hash cmdlet.
//
// Creation Date: Dec 9, 2006
//---------------------------------------------------------------------
using System;
using System.Globalization;
using System.Text;

namespace Pscx.Commands.IO
{
    public class HashInfo
    {
        private string _path;
        private string _algorithm;
        private byte[] _hash;
        private string _hashString;

        public HashInfo(string path, string algorithm, string hashString, byte[] hash)
        {
            _path = ((path == null) ? "" : path);
            _algorithm = algorithm;
            _hashString = hashString;
            _hash = hash;
        }

        public string Path
        {
            get { return _path; }
        }

        public string Algorithm
        {
            get { return _algorithm; }
        }

        public string HashString
        {
            get { return _hashString; }
        }

        public byte[] Hash
        {
            get { return _hash; }
        }

        public override string ToString()
        {
            return _hashString;
        }

        public string ToString(string format)
        {
            if (String.IsNullOrEmpty(format)) throw new ArgumentNullException("format");
            return ToStringImpl(format, null);
        }

        public string ToString(IFormatProvider provider)
        {
            if (provider == null) throw new ArgumentNullException("provider");
            return ToStringImpl(null, provider);
        }

        public string ToString(string format, IFormatProvider provider)
        {
            if (String.IsNullOrEmpty(format)) throw new ArgumentNullException("format");
            if (provider == null) throw new ArgumentNullException("provider");
            return ToStringImpl(format, provider);
        }

        private string ToStringImpl(string format, IFormatProvider provider)
        {
            if (format == null)
            {
                format = "X2";
            }

            if (provider == null)
            {
                provider = CultureInfo.CurrentCulture.NumberFormat;
            }

            StringBuilder strBld = new StringBuilder();
            foreach (byte b in _hash)
            {
                strBld.AppendFormat(provider, "{0:" + format + "}", b);
            }
            return strBld.ToString();
        }
    }
}
