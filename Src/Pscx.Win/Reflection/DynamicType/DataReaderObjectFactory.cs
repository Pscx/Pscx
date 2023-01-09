//---------------------------------------------------------------------
// Author: Harley Green
//
// Description: Cmdlet to get data from Sql Server databases
//
// Creation Date: 2008/8/20
//---------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Wintellect.PowerCollections;

namespace Pscx.Win.Reflection.DynamicType
{
    public class DataReaderObjectFactory : IEnumerable
    {
        private readonly IDataReader _dataReader;

        public DataReaderObjectFactory(IDataReader dataReader)
        {
            this._dataReader = dataReader;
        }

        public Type ReturnType { get; set; }

        public bool IgnoreCase { get; set;  }

        public IEnumerator GetEnumerator()
        {
            if (_dataReader.FieldCount == 1)
            {
                while (_dataReader.Read())
                {
                    yield return _dataReader[0];
                }
            }

            else
            {
                int fieldCount = _dataReader.FieldCount;

                var properties = new List<Pair<string, Type>>();
                var columns = new List<string>();
                for (int i = 0; i < fieldCount; i++)
                {
                    string name = _dataReader.GetName(i);
                    properties.Add(new Pair<string, Type>(name, _dataReader.GetFieldType(i)));
                    columns.Add(name);
                }
                var t = ReturnType ?? new DataTypeBuilder("Pscx").CreateType(properties);
                while (_dataReader.Read())
                {
                    var target = Activator.CreateInstance(t);
                    new PropertySetter(t).SetValues(target, new DataReaderIndexer(_dataReader, columns), IgnoreCase);
                    yield return target;
                }
            }
        }
    }
}