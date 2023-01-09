using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Threading;

namespace Pscx.SIUnits
{
    [TypeConverter(typeof(NonSIUnitConverter))]
    public abstract class NonSIUnit
    {
        private readonly Type _type;
        private readonly Double _size;
        private readonly ReadOnlyCollection<String> _units;

        protected NonSIUnit(Type type, Double size, params String[] units)
        {
            _type = type;
            _size = size;
            _units = new ReadOnlyCollection<String>(units);
        }

        public Double Size
        {
            get { return _size; }
        }

        public ReadOnlyCollection<String> Units
        {
            get { return _units; }
        }

        public Object ToMetric(Double value)
        {
            return Activator.CreateInstance(_type, value * Size);
        }

        public override String ToString()
        {
            if (_units.Count == 0)
            {
                return GetType().Name.ToLowerInvariant();
            }

            return _units[0];
        }

        public static Boolean TryParse(String str, out NonSIUnit result)
        {
            return UnitsCache.TryGetValue(str, out result);            
        }

        public static NonSIUnit Parse(String str)
        {
            NonSIUnit result;
            Boolean success = TryParse(str, out result);

            PscxArgumentException.ThrowIf(!success, Resources.Errors.UnknownNonSIUnit, str);
            
            return result;
        }

        public static readonly NonSIUnit<Length> Inch = new NonSIUnit<Length>(0.0254, "in");
        public static readonly NonSIUnit<Length> Foot = new NonSIUnit<Length>(0.3048, "ft");
        public static readonly NonSIUnit<Length> Yard = new NonSIUnit<Length>(0.9144, "yd");
        public static readonly NonSIUnit<Length> Mile = new NonSIUnit<Length>(1609.344, "mi");

        public static readonly NonSIUnit<Mass> Pound = new NonSIUnit<Mass>(453.59237, "lb", "lbm");
        public static readonly NonSIUnit<Mass> Ounce = new NonSIUnit<Mass>(28.349523125, "oz");
        public static readonly NonSIUnit<Mass> Carat = new NonSIUnit<Mass>(0.2);
        public static readonly NonSIUnit<Mass> Dram = new NonSIUnit<Mass>(1.7718451953125);
        public static readonly NonSIUnit<Mass> Grain = new NonSIUnit<Mass>(0.06479891, "gr");

        public static readonly NonSIUnit<Energy> ElectronVolt = new NonSIUnit<Energy>(1.60217653e-19, "eV");
        public static readonly NonSIUnit<Energy> Calorie = new NonSIUnit<Energy>(4.1868, "cal");
        public static readonly NonSIUnit<Energy> FootPound = new NonSIUnit<Energy>(4.1868, "ftlb");
        public static readonly NonSIUnit<Energy> BritishTermalUnit = new NonSIUnit<Energy>(1055.056, "BTU");

        public static readonly NonSIUnit<Pressure> Atmosphere = new NonSIUnit<Pressure>(101325, "atm");
        public static readonly NonSIUnit<Pressure> Bar = new NonSIUnit<Pressure>(100000, "bar");
        public static readonly NonSIUnit<Pressure> Psi = new NonSIUnit<Pressure>(6894.757, "psi");
        public static readonly NonSIUnit<Pressure> Torr = new NonSIUnit<Pressure>(133.322, "torr", "mmHg");

        public static readonly NonSIUnit<Area> SquareFoot = new NonSIUnit<Area>(0.092903, "sqft");
        public static readonly NonSIUnit<Area> SquareMile = new NonSIUnit<Area>(2589988, "sqmi");
        public static readonly NonSIUnit<Area> Acre = new NonSIUnit<Area>(4046.856, "acres", "acre");

        private static IDictionary<String, NonSIUnit> UnitsCache
        {
            get
            {
                if (_unitsCache == null)
                {
                    var cache = CreateUnitsCache();
                    Thread.MemoryBarrier();

                    _unitsCache = cache;
                }

                return _unitsCache;
            }
        }

        private static Dictionary<String, NonSIUnit> CreateUnitsCache()
        {
            var cache = new Dictionary<String, NonSIUnit>(StringComparer.InvariantCultureIgnoreCase);

            const BindingFlags PublicStatic = BindingFlags.Public | BindingFlags.Static;

            foreach (var field in typeof(NonSIUnit).GetFields(PublicStatic))
            {
                if (typeof(NonSIUnit).IsAssignableFrom(field.FieldType))
                {
                    var value = (NonSIUnit)field.GetValue(null);

                    cache[field.Name] = value;

                    foreach (var unit in value.Units)
                    {
                        cache[unit] = value;
                    }
                }
            }

            return cache;
        }

        private static Dictionary<String, NonSIUnit> _unitsCache;
    }
}
