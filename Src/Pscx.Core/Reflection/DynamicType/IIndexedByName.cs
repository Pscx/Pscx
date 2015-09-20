//---------------------------------------------------------------------
// Author: Harley Green
//
// Description: Cmdlet to get data from Sql Server databases
//
// Creation Date: 2008/8/20
//---------------------------------------------------------------------
namespace Pscx.Reflection.DynamicType
{
    public interface IIndexedByName
    {
        bool TryGetValue(string name, out object value, bool ignoreCase);
    }
}