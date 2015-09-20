//---------------------------------------------------------------------
// Author: Keith Hill, jachymko
//
// Description: Class containing parameter help data.
//
// Creation Date: Dec 23, 2006
//---------------------------------------------------------------------
using System;
using System.Management.Automation;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;

namespace Pscx.Commands.SnapinHelp
{
    [XmlRoot("Parameter")]
    public sealed class ParameterInfo
    {
        [XmlAttribute]
        public string Name;

        [XmlIgnore]
        public Type Type;
        
        [XmlElement]
        public string DefaultValue;
        
        [XmlElement]
        public string Description;
        
        [XmlIgnore]
        public string ParameterSetName;
        
        [XmlAttribute]
        public bool Required;
        
        [XmlAttribute]
        public int Position;
        
        [XmlAttribute]
        public bool AcceptsWildcards;
        
        [XmlAttribute]
        public bool ValueFromPipeline;
        
        [XmlAttribute]
        public bool ValueFromPipelineByPropertyName;

        [XmlElement]
        public string ValueType;

        public ParameterInfo()
        {
            Position = -1;
        }

        public ParameterInfo(PropertyInfo pi, ParameterAttribute pa)
        {
            Name = pi.Name;
            Type = pi.PropertyType;

            ValueType = Type.Name;
            Position = pa.Position;
            Required = pa.Mandatory;
            ParameterSetName = pa.ParameterSetName;
            ValueFromPipeline = pa.ValueFromPipeline;
            ValueFromPipelineByPropertyName = pa.ValueFromPipelineByPropertyName;
        
            if(Position < 0)
                Position = -1;
        }

        XmlNode FindNode(XmlDocument doc)
        {
            return doc.SelectSingleNode("/Cmdlet/Parameters/Parameter[@Name='" + Name + "']");
        }

        string ReadNodeText(XmlNode my, string xpath)
        {
            XmlNode node = my.SelectSingleNode(xpath);
            if(node != null)
                return node.InnerText;

            return string.Empty;
        }

        internal void MergeWithLocalizedXml(XmlDocument document)
        {
            XmlNode my = FindNode(document);
            if(my != null)
            {
                Description = ReadNodeText(my, "Description");
                DefaultValue = ReadNodeText(my, "DefaultValue");
            }
        }

        internal void WriteTo(XmlWriter output)
        {
            serializer.Serialize(output, this);
        }

        readonly static XmlSerializer serializer = new XmlSerializer(typeof(ParameterInfo));
    }
}
