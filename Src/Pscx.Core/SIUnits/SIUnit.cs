using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Pscx.SIUnits
{
    public sealed class SIUnit
    {
        private readonly String _symbol;
        private readonly ReadOnlyCollection<Int32> _negativePrefixes;
        private readonly ReadOnlyCollection<Int32> _positivePrefixes;

        private SIUnit(String symbol, Int32[] negativePrefixes, Int32[] positivePrefixes)
        {
            _symbol = symbol;
            _negativePrefixes = new ReadOnlyCollection<Int32>(negativePrefixes);
            _positivePrefixes = new ReadOnlyCollection<Int32>(positivePrefixes);
        }

        public String Symbol
        {
            get { return _symbol; }
        }

        public ICollection<Int32> PositivePrefixes
        {
            get { return _positivePrefixes; }
        }

        public ICollection<Int32> NegativePrefixes
        {
            get { return _negativePrefixes; }
        }

        public static readonly SIUnit Meter = new SIUnit(
            "m",
            new[] { -2, -3, -6, -9 },
            new[] { 3 }
        );

        public static readonly SIUnit Gram = new SIUnit(
            "g",
            new[] { -3, -6, -9, -12 },
            new[] { 3 }
        );

        public static readonly SIUnit Joule = new SIUnit(
            "J",
            new[] { -3, -6, -9, -12 },
            new[] { 3, 6, 9, 12 }
        );

        public static readonly SIUnit Pascal = new SIUnit(
            "Pa",
            new[] { -3 },
            new[] { 2, 3, 6, 9 }
        );
        
    }
}
