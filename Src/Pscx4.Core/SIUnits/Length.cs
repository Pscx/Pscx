using System;

namespace Pscx.SIUnits
{
    [Serializable]
    public struct Length : IComparable<Length>, IComparable, IEquatable<Length>, IFormattable
    {
        private readonly Double _value;

        public Length(Double value)
        {
            _value = value;
        }

        public Double Inches
        {
            get { return NonSIUnit.Inch.FromMetric(_value); } 
        }

        public Double Feet
        {
            get { return NonSIUnit.Foot.FromMetric(_value); }
        }

        public Double Yards
        {
            get { return NonSIUnit.Yard.FromMetric(_value); }
        }

        public Double Miles
        {
            get { return NonSIUnit.Mile.FromMetric(_value); }
        }

        public Int32 CompareTo(Length other)
        {
            return _value.CompareTo(other._value);
        }

        public Boolean Equals(Length other)
        {
            return _value.Equals(other._value);
        }

        public override Boolean Equals(Object obj)
        {
            if (obj is Length)
            {
                return Equals((Length)(obj));
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
            return SIHelper.ToString(_value, SIUnit.Meter, format, formatProvider);
        }

        Int32 IComparable.CompareTo(Object obj)
        {
            if (obj is Length)
            {
                return CompareTo((Length)(obj));
            }

            throw PscxArgumentException.ObjectMustBeOfType("obj", typeof(Length));
        }

        public static Length FromInches(Double inches)
        {
            return NonSIUnit.Inch.ToMetric(inches);
        }

        public static Length FromFeet(Double feet)  
        {
            return NonSIUnit.Foot.ToMetric(feet);
        }

        public static Length FromYards(Double yards)
        {
            return NonSIUnit.Yard.ToMetric(yards);
        }

        public static Length FromMiles(Double miles)
        {
            return NonSIUnit.Mile.ToMetric(miles);
        }

        public static explicit operator Double(Length l)
        {
            return l._value;
        }
    }
}
