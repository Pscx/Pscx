using System;
using System.Collections.Generic;
using System.Text;

namespace Pscx.SIUnits
{
    [Serializable]
    public struct Pressure : IComparable<Pressure>, IComparable, IEquatable<Pressure>, IFormattable
    {
        private readonly Double _value;

        public Pressure(Double value)
        {
            _value = value;
        }

        public Double Atmospheres
        {
            get { return NonSIUnit.Atmosphere.FromMetric(_value); }
        }

        public Double Bars
        {
            get { return NonSIUnit.Bar.FromMetric(_value); }
        }

        public Double Psi
        {
            get { return NonSIUnit.Psi.FromMetric(_value); }
        }
        
        public Double Torrs
        {
            get { return NonSIUnit.Torr.FromMetric(_value); }
        }

        public Int32 CompareTo(Pressure other)
        {
            return _value.CompareTo(other._value);
        }

        public Boolean Equals(Pressure other)
        {
            return _value.Equals(other._value);
        }

        public override Boolean Equals(Object obj)
        {
            if (obj is Pressure)
            {
                return Equals((Pressure)(obj));
            }

            return false;
        }

        public override Int32 GetHashCode()
        {
            return _value.GetHashCode();
        }

        public override String ToString()
        {
            return ToString(null, null);
        }

        public String ToString(IFormatProvider formatProvider)
        {
            return ToString(null, formatProvider);
        }

        public String ToString(String format, IFormatProvider formatProvider)
        {
            return SIHelper.ToString(_value, SIUnit.Pascal, format, formatProvider);
        }

        Int32 IComparable.CompareTo(Object obj)
        {
            if (obj is Pressure)
            {
                return CompareTo((Pressure)(obj));
            }

            throw PscxArgumentException.ObjectMustBeOfType("obj", typeof(Pressure));
        }

        public static Pressure FromAtmospheres(Double atmospheres)
        {
            return NonSIUnit.Atmosphere.ToMetric(atmospheres);
        }

        public static Pressure FromBars(Double bars)
        {
            return NonSIUnit.Bar.ToMetric(bars);
        }

        public static Pressure FromPsi(Double psi)
        {
            return NonSIUnit.Psi.ToMetric(psi);
        }

        public static Pressure FromTorrs(Double torrs)
        {
            return NonSIUnit.Torr.ToMetric(torrs);
        }

        public static explicit operator Double(Pressure e)
        {
            return e._value;
        }
    }
}
