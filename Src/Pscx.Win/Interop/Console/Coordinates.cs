//---------------------------------------------------------------------
// Original Author: jachymko
//
// Description: Structures for Win32 Console API.
//
// Creation Date: Jan 14, 2007
//---------------------------------------------------------------------

using System.Globalization;
using System.Runtime.InteropServices;

namespace Pscx.Win.Interop.Console
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Coordinates
    {
        private short _x;
        private short _y;

        public Coordinates(short x, short y)
        {
            _x = x;
            _y = y;
        }

        public short X
        {
            get { return _x; }
            set { _x = value; }
        }

        public short Y
        {
            get { return _y; }
            set { _y = value; }
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0},{1}", _x, _y);
        }
    }
}
