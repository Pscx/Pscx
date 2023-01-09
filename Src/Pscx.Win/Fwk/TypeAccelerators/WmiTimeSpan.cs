using System;
using System.Management;

namespace Pscx.Win.Fwk.TypeAccelerators
{
    public struct WmiTimeSpan
    {
        private readonly TimeSpan _value;

        public WmiTimeSpan(TimeSpan value)
        {
            _value = value;
        }

        public override String ToString()
        {
            return ManagementDateTimeConverter.ToDmtfTimeInterval(_value);
        }
    }
}
