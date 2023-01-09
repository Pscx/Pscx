using System;

namespace Pscx.SIUnits
{
    [Serializable]
    public struct Energy : IComparable<Energy>, IComparable, IEquatable<Energy>, IFormattable
    {
        private readonly Double _value;

        public Energy(Double value)
        {
            _value = value;
        }

        public Double ElectronVolts
        {
            get { return NonSIUnit.ElectronVolt.FromMetric(_value); }
        }
        
        public Double Calories
        {
            get { return NonSIUnit.Calorie.FromMetric(_value); }
        }

        public Double FootPounds
        {
            get { return NonSIUnit.FootPound.FromMetric(_value); }
        }

        public Double BritishTermalUnits
        {
            get { return NonSIUnit.BritishTermalUnit.FromMetric(_value); }
        }

        public Int32 CompareTo(Energy other)
        {
            return _value.CompareTo(other._value);
        }

        public Boolean Equals(Energy other)
        {
            return _value.Equals(other._value);
        }

        public override Boolean Equals(Object obj)
        {
            if (obj is Energy)
            {
                return Equals((Energy)(obj));
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
            return SIHelper.ToString(_value, SIUnit.Joule, format, formatProvider);
        }

        Int32 IComparable.CompareTo(Object obj)
        {
            if (obj is Energy)
            {
                return CompareTo((Energy)(obj));
            }

            throw PscxArgumentException.ObjectMustBeOfType("obj", typeof(Energy));
        }

        public static Energy FromElectronVolts(Double electronVolts)
        {
            return NonSIUnit.ElectronVolt.ToMetric(electronVolts);
        }
        
        public static Energy FromCalories(Double calories)
        {
            return NonSIUnit.Calorie.ToMetric(calories);
        }
        
        public static Energy FromFootPounds(Double footPounds)
        {
            return NonSIUnit.FootPound.ToMetric(footPounds);
        }
        
        public static Energy FromBritishTermalUnits(Double britishTermalUnits)
        {
            return NonSIUnit.BritishTermalUnit.ToMetric(britishTermalUnits);
        }

        public static explicit operator Double(Energy e)
        {
            return e._value;
        }
    }
}
