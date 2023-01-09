using System;
using System.Management;

namespace Pscx.Win.Fwk.TypeAccelerators
{
    public struct WmiDateTime
    {
        private readonly DateTime _value;

        public WmiDateTime(DateTime value)
        {
            _value = value;
        }

        public override String ToString()
        {
            return ManagementDateTimeConverter.ToDmtfDateTime(_value);
        }
    }
}
