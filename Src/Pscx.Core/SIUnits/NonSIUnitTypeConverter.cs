using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;

namespace Pscx.SIUnits
{
    public sealed class NonSIUnitTypeConverter : TypeConverter
    {
        public override Boolean CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(String))
            {
                return true;
            }

            return base.CanConvertFrom(context, sourceType);
        }

        public override Boolean CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(String))
            {
                return true;
            }

            return base.CanConvertTo(context, destinationType);
        }

        public override Object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, Object value)
        {
            if (value is String)
            {
                const BindingFlags PublicStatic = BindingFlags.Public | BindingFlags.Static;

                foreach (var field in typeof(NonSIUnit).GetFields(PublicStatic))
                {
                    if (field.FieldType == typeof(NonSIUnit))
                    {
                        var unit = (NonSIUnit)field.GetValue(null);


                    }
                }
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override Object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, Object value, Type destinationType)
        {
            var unit = (value as NonSIUnit);

            if (unit != null)
            {
                if (destinationType == typeof(String))
                {
                    return unit.Units[0];
                }
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }

        private static IEnumerable<NonSIUnit> GetUnits()
        {
            
        }
    }
}
