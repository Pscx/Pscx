using System;

namespace Pscx.SIUnits
{
    public sealed class NonSIUnit<T> : NonSIUnit
    {
        internal NonSIUnit(Double size, params String[] units)
            : base(typeof(T), size, units)
        {
        }

        public new T ToMetric(Double value)
        {
            return (T)base.ToMetric(value);
        }

        public Double FromMetric(Double value)
        {
            return (value / Size);
        }
    }
}
