//---------------------------------------------------------------------
// Author: Keith Hill, jachymko
//
// Description: Class to implement the Format-Xml cmdlet.
//
// Creation Date: Sept 9, 2006
//
// Copyright (C) 2006 PowerShell Community Extensions Developers
//---------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Management.Automation;
using System.Security;
using System.Text;
using System.Xml;
using System.Xml.XPath;

using Microsoft.PowerShell.Commands;

namespace Pscx.Commands.Xml
{
    [OutputType(typeof(string))]
    [Cmdlet(PscxVerbs.Format, PscxNouns.Xml, DefaultParameterSetName = ParameterSetPath)]
    [Description("Pretty print for XML files and XmlDocument objects.")]    
    [RelatedLink(typeof(ConvertXmlCommand))]
    [RelatedLink(typeof(TestXmlCommand))]
    [ProviderConstraint(typeof(FileSystemProvider))]
    public class FormatXmlCommand : XmlCommandBase
    {
        private SwitchParameter _newLineOnAttributes;
        private SwitchParameter _omitXmlDecl;
        private ConformanceLevel _conformanceLevel = ConformanceLevel.Auto;
        private string _indentStr;

        [Parameter(HelpMessage = "Write attributes on a new line.")]
        public SwitchParameter AttributesOnNewLine
        {
            get { return _newLineOnAttributes; }
            set { _newLineOnAttributes = value; }
        }

        [Parameter(HelpMessage = "Omit the XML declaration element.")]
        public SwitchParameter OmitXmlDeclaration
        {
            get { return _omitXmlDecl; }
            set { _omitXmlDecl = value; }
        }

        [Parameter(HelpMessage = "Conformance level for XML.")]
        [DefaultValue("Auto")]
        public ConformanceLevel ConformanceLevel
        {
            get { return _conformanceLevel; }
            set { _conformanceLevel = value; }
        }

        [Parameter(HelpMessage = "The string to use for indenting.")]
        public string IndentString
        {
            get { return _indentStr; }
            set { _indentStr = value; }
        }

        protected override XmlReaderSettings XmlReaderSettings
        {
            get
            {
                XmlReaderSettings settings = base.XmlReaderSettings;
                settings.ConformanceLevel = _conformanceLevel;
                return settings;
            }
        }

        protected override XmlWriterSettings XmlWriterSettings
        {
            get
            {
                XmlWriterSettings settings = base.XmlWriterSettings;
                settings.ConformanceLevel = _conformanceLevel;
                settings.OmitXmlDeclaration = _omitXmlDecl;
                settings.NewLineOnAttributes = _newLineOnAttributes;
                
                if (_indentStr != null)
                {
                    settings.IndentChars = _indentStr;
                }

                return settings;
            }
        }

        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            RegisterInputType<XPathNavigator>(ProcessXPathNavigator);
        }

        protected override void ProcessXmlReader(XmlReader xmlReader, XmlWriter xmlWriter)
        {
            try
            {
                while (!xmlReader.EOF)
                {
                    xmlWriter.WriteNode(xmlReader, false);
                }
            }
            catch (PipelineStoppedException)
            {
                throw;
            }
            catch (Exception ex)
            {
                ErrorHandler.WriteXmlError(ex);
            }
        }

        private void ProcessXPathNavigator(XPathNavigator xpathNavigator)
        {
            try
            {
                using (StringWriter stringWriter = new StringWriter())
                {
                    using (XmlWriter xmlWriter = XmlWriter.Create(stringWriter, XmlWriterSettings))
                    {
                        xpathNavigator.WriteSubtree(xmlWriter);
                    }

                    WriteObject(stringWriter.ToString());
                }
            }
            catch (PipelineStoppedException)
            {
                throw;
            }
            catch (Exception ex)
            {
                ErrorHandler.WriteXmlError(ex);
            }
        }
    }
}