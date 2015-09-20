using System;
using System.Globalization;

namespace Pscx.TypeAccelerators
{
    public struct Hex
    {
        private readonly Int32 _value;

        public Hex(Int32 value)
        {
            _value = value;
        }

        public override String ToString()
        {
            return String.Format(CultureInfo.InvariantCulture, "0x{0:x}", _value);
        }
    }
}
