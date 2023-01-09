//---------------------------------------------------------------------
// Author: Harley Green
//
// Description: Class for converting data records to custom objects
//
// Creation Date: 2008/3/8
//---------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Wintellect.PowerCollections;

namespace Pscx.Win.Reflection.DynamicType
{
    public class DataTableObjectFactory : IEnumerable
    {
        private readonly DataSet _dataSet;

        public DataTableObjectFactory(DataSet dataSet)
        {
            this._dataSet = dataSet;
        }

        public IEnumerator GetEnumerator()
        {
            if (_dataSet.Tables.Count == 1 && _dataSet.Tables[0].Columns.Count == 1 && _dataSet.Tables[0].Rows.Count == 1)
                // scalar
            {
                yield return _dataSet.Tables[0].Rows[0][0];
            }
            else
            {
                foreach (DataTable table in _dataSet.Tables)
                {
                    var properties = new List<Pair<string, Type>>();
                    foreach (DataColumn column in table.Columns)
                    {                        
                        properties.Add(new Pair<string, Type>(column.ColumnName, column.DataType));
                    }
                    var t = new DataTypeBuilder("Pscx").CreateType(properties);
                    foreach (DataRow row in table.Rows)
                    {
                        var target = Activator.CreateInstance(t);
                        new PropertySetter(t).SetValues(target, new DataRowIndexer(row), false);
                        yield return target;
                    }
                }
            }
        }
    }
}