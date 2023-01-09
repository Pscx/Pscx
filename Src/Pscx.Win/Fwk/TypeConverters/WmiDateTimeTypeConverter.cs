//---------------------------------------------------------------------
// Author: jachymko, based on Abhishek's article
//         http://abhishek225.spaces.live.com/Blog/cns!13469C7B7CE6E911!191.entry
//
// Description: Converter for the DMTF date time format (used by WMI)
//
// Creation Date: Jan 23, 2007
//---------------------------------------------------------------------

using System;
using System.Management;
using System.Management.Automation;

namespace Pscx.Win.Fwk.TypeConverters
{
    public sealed class WmiDateTimeTypeConverter : PSTypeConverter
    {
        public override Boolean CanConvertFrom(Object sourceValue, Type destinationType)
        {
            var source = sourceValue as String;

            if (source == null)
            {
                return false;
            }

            if(destinationType == typeof(DateTime))
            {
                return SwallowException(delegate
                {
                    ManagementDateTimeConverter.ToDateTime(source);
                });
            }

            if (destinationType == typeof(TimeSpan))
            {
                return SwallowException(delegate
                {
                    ManagementDateTimeConverter.ToTimeSpan(source);
                });
            }

            return false;
        }

        public override Object ConvertFrom(Object sourceValue, Type destinationType, IFormatProvider formatProvider, Boolean ignoreCase)
        {
            var source = sourceValue as String;

            if (source == null)
            {
                return false;
            }

            if (destinationType == typeof(DateTime))
            {
                return ManagementDateTimeConverter.ToDateTime(source);
            }

            if (destinationType == typeof(TimeSpan))
            {
                return ManagementDateTimeConverter.ToTimeSpan(source);
            }

            throw new InvalidCastException();
        }

        public override Boolean CanConvertTo(Object sourceValue, Type destinationType)
        {
            return false;
        }

        public override Object ConvertTo(Object sourceValue, Type destinationType, IFormatProvider formatProvider, Boolean ignoreCase)
        {
            throw new InvalidCastException();
        }

        private Boolean SwallowException(Action action)
        {
            return SwallowException<Exception>(action);
        }

        private Boolean SwallowException<TException>(Action action) where TException : Exception
        {
            try
            {
                action();
                return true;
            }
            catch (TException)
            {
                return false;
            }
        }
    }
}
