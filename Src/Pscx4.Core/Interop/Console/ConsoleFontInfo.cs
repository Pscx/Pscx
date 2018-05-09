//---------------------------------------------------------------------
// Original Author: jachymko
//
// Description: Structures for Win32 Console API.
//
// Creation Date: Mar 12, 2007
//---------------------------------------------------------------------
using System;
using System.Runtime.InteropServices;

namespace Pscx.Interop
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ConsoleFontInfo
    {
        public int nFont;
        public Coordinates dwFontSize;
    }


}
