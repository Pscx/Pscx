//---------------------------------------------------------------------
// Author: Keith Hill
//
// Description: Attributes to use on PSCX cmdlets to feed the 
//              GetCmdletMaml which produces the MAML help file for
//              PSCX.
//
// Creation Date: Dec 10, 2006
//---------------------------------------------------------------------
using System;
using System.Management.Automation;

namespace Pscx
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public class DetailedDescriptionAttribute : Attribute
    {
        private string _text;

        public DetailedDescriptionAttribute(string text)
        {
            _text = text;
        }

        public string Text
        {
            get { return _text; }
            set { _text = value; }
        }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    public class RelatedLinkAttribute : Attribute
    {
        private string _text;

        public RelatedLinkAttribute(Type cmdletType)
        {
            CmdletAttribute attr = Utils.GetAttribute<CmdletAttribute>(cmdletType);
            PscxArgumentException.ThrowIfIsNull(attr, "The type {0} is not a cmdlet.", cmdletType);

            _text = attr.VerbName + '-' + attr.NounName;
        }

        public RelatedLinkAttribute(string text)
        {
            _text = text;
        }

        public string Text
        {
            get { return _text; }
            set { _text = value; }
        }
    }

    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public class AcceptsWildcardsAttribute : Attribute
    {
        private bool _value;

        public AcceptsWildcardsAttribute(bool value)
        {
            _value = value;
        }

        public bool Value
        {
            get { return _value; }
            set { _value = value; }
        }
    }
}

