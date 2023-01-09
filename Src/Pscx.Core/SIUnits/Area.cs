using System;

namespace Pscx.SIUnits
{
    [Serializable]
    public struct Area : IComparable<Area>, IComparable, IEquatable<Area>, IFormattable
    {
        private readonly Double _value;

        public Area(Double value)
        {
            _value = value;
        }

        public Double SquareFeet
        {
            get { return NonSIUnit.Inch.FromMetric(_value); }
        }

        public Double Acres
        {
            get { return NonSIUnit.Foot.FromMetric(_value); }
        }

        public Double SquareMiles
        {
            get { return NonSIUnit.Yard.FromMetric(_value); }
        }

        public Double Miles
        {
            get { return NonSIUnit.Mile.FromMetric(_value); }
        }

        public Int32 CompareTo(Area other)
        {
            return _value.CompareTo(other._value);
        }

        public Boolean Equals(Area other)
        {
            return _value.Equals(other._value);
        }

        public override Boolean Equals(Object obj)
        {
            if (obj is Area)
            {
                return Equals((Area)(obj));
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
            return SIHelper.ToString(_value, SIUnit.SquareMeter, format, formatProvider);
        }

        Int32 IComparable.CompareTo(Object obj)
        {
            if (obj is Area)
            {
                return CompareTo((Area)(obj));
            }

            throw PscxArgumentException.ObjectMustBeOfType("obj", typeof(Area));
        }

        public static Area FromSquareFeet(Double sqft)
        {
            return NonSIUnit.SquareFoot.ToMetric(sqft);
        }

        public static Area FromSquareMiles(Double sqmi)
        {
            return NonSIUnit.SquareMile.ToMetric(sqmi);
        }

        public static Area FromAcres(Double acres)
        {
            return NonSIUnit.Acre.ToMetric(acres);
        }

        public static explicit operator Double(Area l)
        {
            return l._value;
        }
    }
}
