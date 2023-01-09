using System;

namespace Pscx.SIUnits
{
    [Serializable]
    public struct Mass : IComparable<Mass>, IComparable, IEquatable<Mass>, IFormattable
    {
        private readonly Double _value;

        public Mass(Double value)
        {
            _value = value;
        }

        public Double Pounds
        {
            get { return NonSIUnit.Pound.FromMetric(_value); }
        }

        public Double Ounces
        {
            get { return NonSIUnit.Ounce.FromMetric(_value); }
        }

        public Double Carats
        {
            get { return NonSIUnit.Carat.FromMetric(_value); }
        }

        public Double Drams
        {
            get { return NonSIUnit.Dram.FromMetric(_value); }
        }

        public Double Grains
        {
            get { return NonSIUnit.Grain.FromMetric(_value); }
        }

        public Int32 CompareTo(Mass other)
        {
            return _value.CompareTo(other._value);
        }

        public Boolean Equals(Mass other)
        {
            return _value.Equals(other._value);
        }

        public override Boolean Equals(Object obj)
        {
            if (obj is Mass)
            {
                return Equals((Mass)(obj));
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
            return SIHelper.ToString(_value, SIUnit.Gram, format, formatProvider);
        }

        Int32 IComparable.CompareTo(Object obj)
        {
            if (obj is Mass)
            {
                return CompareTo((Mass)(obj));
            }

            throw PscxArgumentException.ObjectMustBeOfType("obj", typeof(Mass));
        }

        public static Mass FromOunces(Double ounces)
        {
            return NonSIUnit.Ounce.ToMetric(ounces);
        }

        public static Mass FromCarats(Double carats)
        {
            return NonSIUnit.Carat.ToMetric(carats);
        }
        
        public static Mass FromDrams(Double drams)
        {
            return NonSIUnit.Dram.ToMetric(drams);
        }
        
        public static Mass FromGrains(Double grains)
        {
            return NonSIUnit.Grain.ToMetric(grains);
        }

        public static explicit operator Double(Mass m)
        {
            return m._value;
        }
    }
}
