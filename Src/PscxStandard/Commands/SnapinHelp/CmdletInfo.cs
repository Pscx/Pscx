//---------------------------------------------------------------------
// Author: Keith Hill, jachymko
//
// Description: Class containing cmdlet help data.
//
// Creation Date: Dec 23, 2006
//---------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;
using System.Xml;

namespace Pscx.Commands.SnapinHelp
{
    public sealed class CmdletInfo
    {
        public Type Type;
        public string Verb;
        public string Noun;
        public string Description;
        public string DetailedDescription;
        
        public List<ParameterInfo> AllParametersParameterSet;
        public Dictionary<String, List<ParameterInfo>> NamedParameterSets;

        public List<XmlNode> InputTypes;
        public List<XmlNode> ReturnTypes;
        public List<XmlNode> Notes;
        public List<XmlNode> Examples;
        public List<String> RelatedLinks;

        public CmdletInfo()
        {
            AllParametersParameterSet = new List<ParameterInfo>();
            NamedParameterSets = new Dictionary<String, List<ParameterInfo>>(StringComparer.OrdinalIgnoreCase);

            ReturnTypes = new List<XmlNode>();
            InputTypes = new List<XmlNode>();
            Notes = new List<XmlNode>();
            Examples = new List<XmlNode>();
            RelatedLinks = new List<String>();
        }

        public CmdletInfo(Type type, CmdletAttribute cmdlet)
            : this()
        {
            Type = type;
            Verb = cmdlet.VerbName;
            Noun = cmdlet.NounName;
        }

        public string Name
        {
            get { return Verb + "-" + Noun; }
        }

        public override string ToString()
        {
            return Name;
        }

        internal void AddParameter(ParameterInfo parameter)
        {
            string set = parameter.ParameterSetName;
            if (set == AllParameterSets)
            {
                AllParametersParameterSet.Add(parameter);
            }
            else
            {
                if (!NamedParameterSets.ContainsKey(set))
                {
                    NamedParameterSets[set] = new List<ParameterInfo>();
                }

                NamedParameterSets[set].Add(parameter);
            }
        }

        internal void MergeWithLocalizedXml(string localizedHelpDirPath)
        {
            XmlDocument localized = OpenLocalizedXml(localizedHelpDirPath);
            if (localized != null)
            {
                Description = ReadNodeText(localized, "/Cmdlet/Description");
                DetailedDescription = ReadNodeText(localized, "/Cmdlet/DetailedDescription");

                AddNodeRange(InputTypes, localized.SelectNodes("/Cmdlet/InputTypes/InputType"));
                AddNodeRange(ReturnTypes, localized.SelectNodes("/Cmdlet/ReturnTypes/ReturnType"));
                AddNodeRange(Notes, localized.SelectNodes("/Cmdlet/Notes/Note"));
                AddNodeRange(Examples, localized.SelectNodes("/Cmdlet/Examples/Example"));

                foreach (ParameterInfo p in GetAllParameters())
                {
                    p.MergeWithLocalizedXml(localized);
                }
            }
        }

        internal IEnumerable<ParameterInfo> GetAllParameters()
        {
            foreach(ParameterInfo p in AllParametersParameterSet)
                yield return p;

            foreach(List<ParameterInfo> set in NamedParameterSets.Values)
                foreach(ParameterInfo p in set)
                    yield return p;
        }

        private XmlDocument OpenLocalizedXml(string localizedHelpDirPath)
        {
            string localizedHelpFilePath = Path.Combine(localizedHelpDirPath, Verb + Noun + ".xml");
            using (Stream stream = new FileStream(localizedHelpFilePath, FileMode.Open, FileAccess.Read))
            {
                if (stream != null)
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(stream);

                    return doc;
                }
            }

            return null;
        }

        private string ReadNodeText(XmlDocument doc, string xpath)
        {
            XmlNode node = doc.SelectSingleNode(xpath);
            if(node != null)
                return node.InnerText;

            return string.Empty;
        }

        private void AddNodeRange(List<XmlNode> destination, XmlNodeList source)
        {
            foreach (XmlNode xn in source)
            {
                destination.Add(xn);
            }
        }

        private const string AllParameterSets = "__AllParameterSets";
    }

}
