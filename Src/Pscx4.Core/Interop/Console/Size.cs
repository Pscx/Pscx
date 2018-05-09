//---------------------------------------------------------------------
// Original Author: jachymko
//
// Description: Structures for Win32 Console API.
//
// Creation Date: Jan 14, 2007
//---------------------------------------------------------------------
using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace Pscx.Interop
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Size
    {
        private short _width;
        private short _height;

        public Size(short width, short height)
        {
            _width = width;
            _height = height;
        }

        public short Width
        {
            get { return _width; }
            set { _width = value; }
        }

        public short Height
        {
            get { return _height; }
            set { _height = value; }
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}x{1}", _width, _height);
        }
    }
}
