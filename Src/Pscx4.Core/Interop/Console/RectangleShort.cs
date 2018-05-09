//---------------------------------------------------------------------
// Original Author: jachymko
//
// Description: Structures for Win32 Console API.
//
// Creation Date: Jan 14, 2007
//---------------------------------------------------------------------
using System;
using System.Runtime.InteropServices;
using System.Globalization;

namespace Pscx.Interop
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RectangleShort
    {
        private short _left;
        private short _top;
        private short _right;
        private short _bottom;

        public short Left
        {
            get { return _left; }
            set { _left = value; }
        }

        public short Top
        {
            get { return _top; }
            set { _top = value; }
        }

        public short Right
        {
            get { return _right; }
            set { _right = value; }
        }

        public short Bottom
        {
            get { return _bottom; }
            set { _bottom = value; }
        }

        public short Width
        {
            get { return (short)(_right - _left); }
            set { _right = (short)(_left + value); }
        }

        public short Height
        {
            get { return (short)(_bottom - _top); }
            set { _bottom = (short)(_top + value); }
        }

        public Coordinates Position
        {
            get { return new Coordinates(_left, _top); }
            set
            {
                Size sz = this.Size;

                _left = value.X;
                _top = value.Y;

                _right = (short)(_left + sz.Width);
                _bottom = (short)(_top + sz.Height);
            }
        }

        public Size Size
        {
            get { return new Size(Width, Height); }
            set
            {
                Width = value.Width;
                Height = value.Height;
            }
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "({0})-({1})", Position, Size);
        }
    }
 

}
