//---------------------------------------------------------------------
// Author: Keith Hill, jachymko
//
// Description: Class to implement the Convet-Xml cmdlet which applies
//              the supplied XSL tranform to the supplied XML.
//
// Creation Date: Sept 24, 2006
//---------------------------------------------------------------------
using System;
using System.ComponentModel;
using System.IO;
using System.Management.Automation;
using System.Xml;
using System.Xml.Xsl;
using Microsoft.PowerShell.Commands;
using Pscx.Core.IO;

namespace Pscx.Commands.Xml
{
    [OutputType(typeof(string))]
    [Cmdlet(VerbsData.Convert, PscxNouns.Xml, DefaultParameterSetName = ParameterSetPath),
     Description("Converts XML through a XSL")]
    [RelatedLink(typeof(TestXmlCommand))]
    [RelatedLink(typeof(FormatXmlCommand))]
    [ProviderConstraint(typeof(FileSystemProvider))]
    public class ConvertXmlCommand : XmlCommandBase
    {
        private XslCompiledTransform _xslTransform;
        private SwitchParameter _enableScript;
        private ConformanceLevel _conformanceLevel = ConformanceLevel.Auto;
        private SwitchParameter _enableDocumentFunction;

        [Parameter(Position = 1, Mandatory = true)]
        [PscxPath(NoGlobbing = true, ShouldExist = true)]
        [ValidateNotNullOrEmpty]
        public PscxPathInfo XsltPath { get; set; }

        [Parameter()]
        [PscxPath(NoGlobbing = true)]
        [ValidateNotNullOrEmpty]
        public PscxPathInfo OutputPath { get; set; }

        [Parameter]
        public SwitchParameter EnableScript
        {
            get { return _enableScript; }
            set { _enableScript = value; }
        }

        [Parameter]
        [DefaultValue("Auto")]
        public ConformanceLevel ConformanceLevel
        {
            get { return _conformanceLevel; }
            set { _conformanceLevel = value; }
        }

        [Parameter]
        public SwitchParameter EnableDocumentFunction
        {
            get { return _enableDocumentFunction; }
            set { _enableDocumentFunction = value; }
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
                // Helpful blog post: http://blogs.msdn.com/eriksalt/archive/2005/07/27/OutputSettings.aspx
                XmlWriterSettings settings = _xslTransform.OutputSettings.Clone();
                settings.CloseOutput = true;
                settings.ConformanceLevel = _conformanceLevel;
                return settings; 
            }
        }

        protected virtual XsltSettings XsltSettings
        {
            get { return new XsltSettings(_enableDocumentFunction, _enableScript); }
        }

        protected override void BeginProcessing()
        {
            base.BeginProcessing();

            try
            {
                _xslTransform = new XslCompiledTransform();
                _xslTransform.Load(XsltPath.ProviderPath, XsltSettings, XmlUrlResolver);
            }
            catch (XsltException exc)
            {
                ErrorHandler.WriteXsltError(exc);
            }
            catch (PipelineStoppedException)
            {
                throw;
            }
            catch (Exception exc)
            {
                ErrorHandler.WriteFileError(XsltPath.ProviderPath, exc);
            }
        }

        protected override void ProcessXmlReader(XmlReader xmlReader)
        {
            if (OutputPath != null)
            {
                using (var fileStream = new FileStream(OutputPath.ProviderPath, FileMode.Create))
                {
                    using (XmlWriter xmlWriter = XmlWriter.Create(fileStream, XmlWriterSettings))
                    {
                        ProcessXmlReader(xmlReader, xmlWriter);
                        fileStream.Flush();
                    }
                }
            }
            else
            {
                using (StringWriter stringWriter = new StringWriter())
                {
                    using (XmlWriter xmlWriter = XmlWriter.Create(stringWriter, XmlWriterSettings))
                    {
                        ProcessXmlReader(xmlReader, xmlWriter);
                    }

                    WriteObject(stringWriter.ToString());
                }
            }
        }

        protected override void ProcessXmlReader(XmlReader xmlReader, XmlWriter xmlWriter)
        {
            try
            {
                _xslTransform.Transform(xmlReader, xmlWriter);
            }
            catch (Exception exc)
            {
                ErrorHandler.WriteXsltError(exc);
            }
        }
    }
}