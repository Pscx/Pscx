//---------------------------------------------------------------------
// Author: Harley Green
//
// Description: Class for setting properties on objects, via reflection
//
// Creation Date: 2008/3/8
//---------------------------------------------------------------------
using System;
using System.Reflection;

namespace Pscx.Reflection.DynamicType
{
    public class PropertySetter
    {
        private readonly PropertyInfo[] _properties;

        public PropertySetter(Type type)
        {
            _properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        }

        public void SetValues(object target, IIndexedByName row, bool ignoreCase)
        {
            foreach (var info in _properties)
            {
                object value;
                if (row.TryGetValue(info.Name, out value, ignoreCase))
                {
                    value = value == DBNull.Value ? null : value;
                    info.SetValue(target, value, null);
                }
            }
        }
    }
}