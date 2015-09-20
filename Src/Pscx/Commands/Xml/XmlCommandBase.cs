using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Management.Automation;
using System.Net;
using System.Text;
using System.Xml;
using Pscx.IO;

namespace Pscx.Commands.Xml
{    
    /// <summary>
    /// Abstract class for Xml processing Cmdlets.
    /// <remarks>Derived Cmdlets should be constrained to the FileSystemProvider using a <see cref="ProviderConstraintAttribute"/></remarks>
    /// </summary>
    public abstract class XmlCommandBase : PscxInputObjectPathCommandBase
    {
        private XmlReaderSettings _xmlReaderSettings;
        private XmlWriterSettings _xmlWriterSettings;
        private XmlUrlResolver _xmlUrlResolver;

        [Parameter(HelpMessage = "Enables document type definition (DTD) processing.")]
        public SwitchParameter EnableDtd { get; set; }

        protected virtual XmlReaderSettings XmlReaderSettings
        {
            get
            {
                if (_xmlReaderSettings == null)
                {
                    _xmlReaderSettings = new XmlReaderSettings {CloseInput = true, IgnoreWhitespace = true};
                    if (EnableDtd)
                    {
                        _xmlReaderSettings.DtdProcessing = DtdProcessing.Parse;
                        _xmlReaderSettings.ValidationType = ValidationType.DTD;
                    }
                }
                return _xmlReaderSettings;
            }
        }

        protected virtual XmlWriterSettings XmlWriterSettings
        {
            get
            {
                if (_xmlWriterSettings == null)
                {
                    _xmlWriterSettings = new XmlWriterSettings {CloseOutput = true, Indent = true};
                }
                return _xmlWriterSettings;
            }
        }

        protected virtual XmlUrlResolver XmlUrlResolver
        {
            get
            {
                if (_xmlUrlResolver == null)
                {
                    _xmlUrlResolver = new XmlUrlResolver {Credentials = CredentialCache.DefaultCredentials};
                }
                return _xmlUrlResolver;
            }
        }

        protected override PscxInputObjectPathSettings InputSettings
        {
            get
            {
                PscxInputObjectPathSettings settings = base.InputSettings;
                settings.ProcessDirectoryInfoAsPath = false;
                return settings;
            }
        }

        protected override void BeginProcessing()
        {
            RegisterInputType<String>(ProcessString);
            RegisterInputType<TextReader>(ProcessTextReader);
            RegisterInputType<XmlNode>(ProcessXmlNode);
            RegisterInputType<XmlReader>(ProcessXmlReader);

            base.BeginProcessing();
        }

        protected override void ProcessPath(PscxPathInfo pscxPath)
        {
            // FileSystemProvider should be enforced on deriving commands since 
            // this code assumes Path points to a file.
            FileHandler.ProcessText(pscxPath.ProviderPath, ProcessTextReader);
        }

        private void ProcessString(string xmlText)
        {
            using (StringReader stringReader = new StringReader(xmlText))
            {
                ProcessTextReader(stringReader);
            }
        }

        private void ProcessTextReader(TextReader textReader)
        {
            using (XmlReader xmlReader = XmlReader.Create(textReader, XmlReaderSettings))
            {
                ProcessXmlReader(xmlReader);
            }
        }

        private void ProcessXmlNode(XmlNode node)
        {
            using (XmlNodeReader xmlReader = new XmlNodeReader(node))
            {
                ProcessXmlReader(xmlReader);
            }
        }

        protected virtual void ProcessXmlReader(XmlReader xmlReader)
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

        protected virtual void ProcessXmlReader(XmlReader xmlReader, XmlWriter xmlWriter)
        {
        }
    }
}
