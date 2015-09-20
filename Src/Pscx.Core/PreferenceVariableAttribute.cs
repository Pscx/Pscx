//---------------------------------------------------------------------
// Authors: Jachym Kouba (jachymko)
//
// Description: Runtime attribute for Pscx Cmdlet
//
//---------------------------------------------------------------------

using System;

namespace Pscx
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class PreferenceVariableAttribute : Attribute
    {
        private readonly string _variableName;
        private readonly object _defaultValue;

        public PreferenceVariableAttribute(string variable)
        {
            _variableName = variable;
        }

        public PreferenceVariableAttribute(string variable, object defaultValue)
            : this(variable)
        {
            _defaultValue = defaultValue;
        }

        public object DefaultValue
        {
            get { return _defaultValue; }
        }

        public string VariableName
        {
            get { return _variableName; }
        }
    }
}