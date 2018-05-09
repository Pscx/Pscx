//---------------------------------------------------------------------
// Author: Harley Green
//
// Description: Cmdlet to get data from Sql Server databases
//
// Creation Date: 2008/8/20
//---------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Data;
using Pscx.Reflection.DynamicType;
using Wintellect.PowerCollections;

namespace Pscx.Reflection.DynamicType
{
    public class DataReaderIndexer : IIndexedByName
    {
        private readonly IDataReader _reader;
        private readonly string[] _columns;

        public DataReaderIndexer(IDataReader reader, IEnumerable<string> columns)
        {
            this._reader = reader;
            this._columns = Algorithms.ToArray(columns);
        }

        public bool TryGetValue(string name, out object value, bool ignoreCase)
        {
            if (Array.Exists(_columns, s => s.Equals(name, ignoreCase ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture)))
            {
                value = _reader[name];
                return true;
            }
            value = null;
            return false;
        }
    }
}