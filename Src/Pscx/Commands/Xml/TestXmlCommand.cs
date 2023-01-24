//---------------------------------------------------------------------
// Author: Keith Hill
//
// Description: Class to implement the Test-Xml cmdlet which checks
//              for well-formed by default and optionally validates 
//              against supplied schema.
//
// Creation Date: Sept 27, 2006
//---------------------------------------------------------------------
using System;
using System.ComponentModel;
using System.IO;
using System.Management.Automation;
using System.Xml;
using System.Xml.Schema;

using Microsoft.PowerShell.Commands;
using Pscx.Core.IO;

namespace Pscx.Commands.Xml
{
    [OutputType(typeof(bool))]
    [Cmdlet(VerbsDiagnostic.Test, PscxNouns.Xml, DefaultParameterSetName = ParameterSetPath),
     Description("Tests for well formedness and optionally validates against XML Schema.")]
    [DetailedDescription("Tests for well formedness and optionally validates against XML Schema.  It doesn't handle specifying the targetNamespace.  To see validation error messages, specify the -Verbose flag.")]
    [RelatedLink(typeof(ConvertXmlCommand))]
    [RelatedLink(typeof(FormatXmlCommand))]
    [ProviderConstraint(typeof(FileSystemProvider))]
    public class TestXmlCommand : XmlCommandBase
    {
        private PscxPathInfo[] _schemaPaths;
        private SwitchParameter _validate;
        private bool _validationErrorOccurred;

        [PscxPath]
        [Parameter(HelpMessage = "Array of paths to the required schema files to perform schema-based validation.")]
        public PscxPathInfo[] SchemaPath
        {
            get { return _schemaPaths; }
            set { _schemaPaths = value; }
        }

        [Parameter(HelpMessage = "Forces schema validation of the XML against inline schema.")]
        public SwitchParameter Validate
        {
            get { return _validate; }
            set { _validate = value; }
        }

        protected override XmlReaderSettings XmlReaderSettings
        {
            get
            {
                XmlReaderSettings settings = base.XmlReaderSettings;
                settings.CheckCharacters = true;
                settings.CloseInput = true;
                settings.IgnoreWhitespace = true;
                return settings;
            }
        }

        protected override void BeginProcessing()
        {
            base.BeginProcessing();

            XmlSchemaSet _xmlSchemaSet = new XmlSchemaSet();
            if (_schemaPaths != null)
            {
                if (_schemaPaths.Length > 0)
                {
                    foreach (PscxPathInfo _schemaPath in _schemaPaths)
                    {
                        using (FileStream stream = new FileStream(_schemaPath.ProviderPath, FileMode.Open, FileAccess.Read))
                        {
                            XmlSchema xmlSchema = XmlSchema.Read(stream, SchemaReadValidationHandler);
                            _xmlSchemaSet.Add(xmlSchema);
                        }
                    }

                    // If the user specified a schema they want validation, so force _validate to true
                    _validate = true;
                }
            }

            if (_validate)
            {
                XmlReaderSettings.ValidationType = ValidationType.Schema;
                XmlReaderSettings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
                XmlReaderSettings.Schemas.Add(_xmlSchemaSet);
                XmlReaderSettings.ValidationEventHandler += SchemaValidationHandler;
            }
        }

        private void SchemaReadValidationHandler(object sender, ValidationEventArgs e)
        {
            if (e.Severity == XmlSeverityType.Error)
            {
                ErrorHandler.WriteXmlSchemaError(e.Exception);
            }
            else
            {
                WriteWarning(e.Message);
            }
        }

        protected override void ProcessXmlReader(XmlReader xmlReader)
        {
            try
            {
                _validationErrorOccurred = false;
                while (xmlReader.Read()) ;
                WriteObject(!_validationErrorOccurred);
            }
            catch (XmlSchemaException ex)
            {
                var filename = "";
                var formatStr = "{0}{1}";
                if (CurrentPscxPathInfo != null && CurrentPscxPathInfo.SourcePath != null)
                {
                    formatStr = "{0}: {1}";
                    filename = System.IO.Path.GetFileName(CurrentPscxPathInfo.SourcePath);
                }
                var msg = String.Format(formatStr, filename, ex.Message);
                WriteVerbose(msg);
                WriteObject(false);
            }
            catch (XmlException ex)
            {
                var filename = "";
                var formatStr = "{0}{1}";
                if (CurrentPscxPathInfo != null && CurrentPscxPathInfo.SourcePath != null)
                {
                    formatStr = "{0}: {1}";
                    filename = System.IO.Path.GetFileName(CurrentPscxPathInfo.SourcePath);
                }
                var msg = String.Format(formatStr, filename, ex.Message);
                WriteWarning(msg);
                WriteObject(false);
            }
            catch (PipelineStoppedException)
            {
                throw;
            }
            catch (Exception ex)
            {
                ErrorHandler.WriteXmlError(ex);
                WriteObject(false);
            }
        }

        private void SchemaValidationHandler(object sender, ValidationEventArgs e)
        {
            if (e.Severity == XmlSeverityType.Error)
            {
                _validationErrorOccurred = true;
            }

            string msg = String.Format("{0}: {1} Line {2}, Position {3}.", 
                e.Severity, e.Message, e.Exception.LineNumber, e.Exception.LinePosition);
            WriteVerbose(msg);
        }
    }
}
