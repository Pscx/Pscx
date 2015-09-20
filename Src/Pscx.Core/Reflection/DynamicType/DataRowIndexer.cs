//---------------------------------------------------------------------
// Author: Harley Green
//
// Description: Cmdlet to get data from Sql Server databases
//
// Creation Date: 2008/8/20
//---------------------------------------------------------------------
using System.Data;
using Pscx.Reflection.DynamicType;

namespace Pscx.Reflection.DynamicType
{
    public class DataRowIndexer : IIndexedByName
    {
        private readonly DataRow _row;

        public DataRowIndexer(DataRow row)
        {
            this._row = row;
        }

        public bool TryGetValue(string name, out object value, bool ignoreCase)
        {
            if (_row.Table.Columns.Contains(name))
            {
                value = _row[name];
                return true;
            }
            value = null;
            return false;
        }
    }
}