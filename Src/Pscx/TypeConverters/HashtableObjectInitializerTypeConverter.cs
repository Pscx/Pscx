using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Text;

namespace Pscx.TypeConverters
{
    public class HashtableObjectInitializerTypeConverter : PSTypeConverter
    {
        #region Overrides of PSTypeConverter

        public override bool CanConvertFrom(object sourceValue, Type destinationType)
        {
            throw new NotImplementedException();
        }

        public override object ConvertFrom(object sourceValue, Type destinationType, IFormatProvider formatProvider, bool ignoreCase)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvertTo(object sourceValue, Type destinationType)
        {
            throw new NotImplementedException();
        }

        public override object ConvertTo(object sourceValue, Type destinationType, IFormatProvider formatProvider, bool ignoreCase)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
