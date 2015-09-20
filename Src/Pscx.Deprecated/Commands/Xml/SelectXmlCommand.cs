//---------------------------------------------------------------------
// Author: Mark Maier
//
// Description: Class to implement the Select-Xml cmdlet.
//
// Creation Date: Jan 9, 2007
//---------------------------------------------------------------------
using System;
using System.ComponentModel;
using System.Management.Automation;
using System.Xml;
using System.Xml.XPath;
using Microsoft.PowerShell.Commands;
using Pscx.Commands.Xml;

namespace Pscx.Deprecated.Commands.Xml
{
    [Cmdlet(VerbsCommon.Select, PscxNouns.Xml, DefaultParameterSetName = ParameterSetPath)]
    [Description("Select elements in XML files and XmlDocument objects with XPath expressions.")]
    [RelatedLink(typeof(FormatXmlCommand))]
    [RelatedLink(typeof(ConvertXmlCommand))]
    [RelatedLink(typeof(TestXmlCommand))]
    [ProviderConstraint(typeof(FileSystemProvider))]
    public class SelectXmlCommand : XmlCommandBase
    {
        private string _xpathExpression;
        private string[] _xmlNamespaces;
        private XPathExpression _compiledXPathExpression;

        [Parameter(Position = 1, Mandatory = true, HelpMessage = "XPath expression for selecting XML elements")]
        public string XPath
        {
            get { return _xpathExpression; }
            set { _xpathExpression = value; }
        }

        [Parameter(HelpMessage = @"Array of XML namespaces like ""prefix = uri""")]
        public string[] Namespace
        {
            get { return _xmlNamespaces; }
            set { _xmlNamespaces = value; }
        }

        protected override XmlReaderSettings XmlReaderSettings
        {
            get
            {
                XmlReaderSettings settings = base.XmlReaderSettings;
                settings.ConformanceLevel = ConformanceLevel.Auto;

                return settings;
            }
        }

        protected override XmlWriterSettings XmlWriterSettings
        {
            get
            {
                XmlWriterSettings settings = base.XmlWriterSettings;
                settings.ConformanceLevel = ConformanceLevel.Auto;

                return settings;
            }
        }

        protected override void BeginProcessing()
        {
            base.BeginProcessing();

            try
            {
                _compiledXPathExpression = XPathExpression.Compile(_xpathExpression);
            }
            catch (XPathException ex)
            {
                ErrorHandler.WriteXPathExpressionError(_xpathExpression, ex);
            }

            RegisterInputType<XPathNavigator>(ProcessXPathNavigator);
        }

        protected override void ProcessXmlReader(XmlReader xmlReader)
        {
            try
            {
                while (!xmlReader.EOF)
                {
                    XPathDocument xpathDocument = new XPathDocument(xmlReader);
                    XPathNavigator xpathNavigator = xpathDocument.CreateNavigator();
                    XPathNodeIterator nodes;

                    if (_xmlNamespaces != null)
                    {
                        XmlNamespaceManager xmlNamespaceMgr = new XmlNamespaceManager(xmlReader.NameTable);
                        foreach (string xmlNamespace in _xmlNamespaces)
                        {
                            string[] tmp = xmlNamespace.Split(new char[] { '=' });
                            xmlNamespaceMgr.AddNamespace(tmp[0].Trim(), tmp[1].Trim());
                        }
                        _compiledXPathExpression.SetContext(xmlNamespaceMgr);
                        nodes = xpathNavigator.Select(_compiledXPathExpression);
                    }
                    else
                    {
                        XPathNavigator namespaceResolver = xpathNavigator.Clone();
                        if (namespaceResolver.NodeType == XPathNodeType.Root)
                        {
                            // Move to doc element where namespaces are commonly defined
                            namespaceResolver.MoveToFirstChild();
                        }
                        nodes = xpathNavigator.Select(_xpathExpression, namespaceResolver);
                    }

                    while (nodes.MoveNext())
                    {
                        WriteObject(nodes.Current.Clone());
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.WriteXmlError(ex);
            }
        }

        private void ProcessXPathNavigator(XPathNavigator xpathNavigator)
        {
            XPathNodeIterator nodes;

            try
            {
                if (_xmlNamespaces != null)
                {
                    XmlNamespaceManager xmlNamespaceMgr = new XmlNamespaceManager(xpathNavigator.NameTable);
                    foreach (string xmlNamespace in _xmlNamespaces)
                    {
                        string[] tmp = xmlNamespace.Split(new char[] { '=' });
                        xmlNamespaceMgr.AddNamespace(tmp[0].Trim(), tmp[1].Trim());
                    }
                    _compiledXPathExpression.SetContext(xmlNamespaceMgr);
                    nodes = xpathNavigator.Select(_compiledXPathExpression);
                }
                else
                {
                    XPathNavigator namespaceResolver = xpathNavigator.Clone();
                    if (namespaceResolver.NodeType == XPathNodeType.Root)
                    {
                        // Move to doc element where namespaces are commonly defined
                        namespaceResolver.MoveToFirstChild();
                    }
                    nodes = xpathNavigator.Select(_xpathExpression, namespaceResolver);
                }

                while (nodes.MoveNext())
                {
                    WriteObject(nodes.Current.Clone());
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.WriteXmlError(ex);
            }
        }
    }
}