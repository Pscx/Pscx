using System;
using System.Collections.Generic;

namespace Pscx.SIUnits
{
    internal static class SIHelper
    {
        public static String ToString(Double value, SIUnit unit, String format, IFormatProvider formatProvider)
        {
            var unitStr = unit.Symbol;

            if (value != 0)
            {
                var scale = 0;
                var log10 = Math.Log10(value);

                if (log10 < 0)
                {
                    value = MakeLarger(value, unit, out scale);
                }
                else if (log10 > 0)
                {
                    value = MakeSmaller(value, unit, out scale);
                }

                if (scale != 0)
                {
                    unitStr = Prefixes[scale].Symbol + unit.Symbol;
                }
            }

            return String.Format(formatProvider, "{0} {1}", value, unitStr);
        }

        private static Double MakeSmaller(Double value, SIUnit unit, out Int32 scale)
        {
            var result = value;
            scale = 0;

            foreach (var prefix in unit.PositivePrefixes)
            {
                var log10 = Math.Log10(result);

                if ((log10 <= 2) || (log10 < prefix))
                {
                    return result;
                }

                result = Math.Pow(10, -prefix) * value;
                scale = prefix;
            }

            return result;
        }

        private static Double MakeLarger(Double value, SIUnit unit, out Int32 scale)
        {
            var result = value;
            scale = 0;

            foreach (var prefix in unit.NegativePrefixes)
            {
                if (Math.Log10(result) >= 0)
                {
                    return result;
                }

                result = Math.Pow(10, -prefix) * value;
                scale = prefix;
            }

            return result;
        }

        private static readonly Dictionary<Int32, SIPrefix> Prefixes = new Dictionary<Int32, SIPrefix>()
        {
            { -24, new SIPrefix("yocto", "y") },
            { -21, new SIPrefix("zepto", "z") },
            { -18, new SIPrefix("atto", "a") },
            { -15, new SIPrefix("femto", "f") },
            { -12, new SIPrefix("pico", "p") },
            { -9, new SIPrefix("nano", "n") },
            { -6, new SIPrefix("micro", Mu) },
            { -3, new SIPrefix("milli", "m") },
            { -2, new SIPrefix("centi", "c") },
            { -1, new SIPrefix("deci", "d") },
            { 1, new SIPrefix("deca", "da") },
            { 2, new SIPrefix("hecto", "h") },
            { 3, new SIPrefix("kilo", "k") },
            { 6, new SIPrefix("mega", "M") },
            { 9, new SIPrefix("giga", "G") },
            { 12, new SIPrefix("tera", "T") },
            { 15, new SIPrefix("peta", "P") },
            { 18, new SIPrefix("exa", "E") },
            { 21, new SIPrefix("zetta", "Z") },
            { 24, new SIPrefix("yotta", "Y") },
        };

        private struct SIPrefix
        {
            public readonly String Prefix;
            public readonly String Symbol;

            public SIPrefix(String prefix, String symbol)
            {
                Prefix = prefix;
                Symbol = symbol;
            }
        }

        private const String Mu = "\u03bc";
    }
}
