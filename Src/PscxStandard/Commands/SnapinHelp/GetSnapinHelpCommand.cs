//---------------------------------------------------------------------
// Author: jachymko, Keith Hill
//
// Description: Class to implement the Get-PSSnapinHelp cmdlet. 
//              Generates a XML file containing all documentation data.
//
// Creation Date: Dec 23, 2006
//---------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;
using System.Reflection;
using System.Xml;
using Microsoft.PowerShell.Commands;
using Pscx.IO;

namespace Pscx.Commands.SnapinHelp
{
    [Cmdlet(VerbsCommon.Get, "PSSnapinHelp", DefaultParameterSetName = ParameterSetPath)]
    [ProviderConstraint(typeof(FileSystemProvider))]
    public partial class GetSnapinHelpCommand : PscxInputObjectPathCommandBase
    {
        private const string NamespaceXmlSchema = "http://www.w3.org/2001/XMLSchema";
        private const string NamespaceXmlSchemaInstance = "http://www.w3.org/2001/XMLSchema-instance";

        private Visitor _visitor;
        private XmlWriter _xmlWriter;
        private List<CmdletInfo> _cmdlets;

        [Parameter(Mandatory=true)]
        [PscxPath(NoGlobbing = true)]
        [ValidateNotNullOrEmpty]
        public PscxPathInfo LocalizedHelpPath { get; set; }

        [Parameter(Mandatory = true)]
        [PscxPath(NoGlobbing = true)]
        [ValidateNotNullOrEmpty]
        public PscxPathInfo OutputPath { get; set; }

        protected override void BeginProcessing()
        {        
            _cmdlets = new List<CmdletInfo>();
            _visitor = new Visitor(this);
            
            RegisterInputType<Assembly>(ProcessAssembly);
            RegisterInputType<PSSnapInInfo>(ProcessSnapIn);

            base.BeginProcessing();
        }

        protected override void ProcessPath(PscxPathInfo pscxPath)
        {
            Assembly assembly = null;
            string filePath = pscxPath.ProviderPath;

            try
            {
                assembly = Assembly.LoadFrom(filePath);
            }
            catch (PipelineStoppedException)
            {
                throw;
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "FailedAssemblyLoadFrom", ErrorCategory.InvalidArgument, filePath));
                return;
            }

            ProcessAssembly(assembly);
        }

        private void ProcessSnapIn(PSSnapInInfo snapinInfo)
        {
            ProcessPath(PscxPathInfo.GetPscxPathInfo(this.SessionState, snapinInfo.ModuleName));
        }

        private void ProcessAssembly(Assembly assembly)
        {
            _visitor.VisitAssembly(assembly);

            using (var fileStream = new FileStream(OutputPath.ProviderPath, FileMode.Create))
            {
                var xmlWriterSettings = new XmlWriterSettings {Indent = true};

                using (_xmlWriter = XmlWriter.Create(fileStream, xmlWriterSettings))
                {
                    _xmlWriter.WriteStartElement("Cmdlets");
                    _xmlWriter.WriteAttributeString("xmlns", "xsd", null, NamespaceXmlSchema);
                    _xmlWriter.WriteAttributeString("xmlns", "xsi", null, NamespaceXmlSchemaInstance);

                    foreach (CmdletInfo cmdletInfo in _cmdlets)
                    {
                        cmdletInfo.MergeWithLocalizedXml(LocalizedHelpPath.ProviderPath);

                        WriteCmdlet(cmdletInfo);
                    }

                    _xmlWriter.WriteEndElement(); // </Cmdlets>
                    _xmlWriter.WriteEndDocument();

                    fileStream.Flush();
                }
            }
        }

        private void WriteCmdlet(CmdletInfo cmdletInfo)
        {
            _xmlWriter.WriteStartElement("Cmdlet");
            _xmlWriter.WriteAttributeString("TypeName", cmdletInfo.Type.FullName);

            string description = cmdletInfo.Description;
            string detailed = cmdletInfo.DetailedDescription;

            if (string.IsNullOrEmpty(detailed)) detailed = description;

            _xmlWriter.WriteElementString("Verb", cmdletInfo.Verb);
            _xmlWriter.WriteElementString("Noun", cmdletInfo.Noun);
            _xmlWriter.WriteElementString("Description", description);
            _xmlWriter.WriteElementString("DetailedDescription", detailed);

            WriteParameterSets(cmdletInfo);
            WriteParameters(cmdletInfo);

            WriteXmlNodeList(cmdletInfo.InputTypes, "InputType");
            WriteXmlNodeList(cmdletInfo.ReturnTypes, "ReturnType");
            WriteXmlNodeList(cmdletInfo.Notes, "Note");
            WriteXmlNodeList(cmdletInfo.Examples, "Example");
            WriteStringList(cmdletInfo.RelatedLinks, "RelatedLink");

            _xmlWriter.WriteEndElement(); // </Cmdlet>
        }

        private void WriteParameterSets(CmdletInfo cmdletInfo)
        {
            _xmlWriter.WriteStartElement("ParameterSets");

            if (cmdletInfo.NamedParameterSets.Count == 0)
            {
                WriteParameterSet(string.Empty, cmdletInfo.AllParametersParameterSet);
            }
            else
            {
                foreach(string setName in cmdletInfo.NamedParameterSets.Keys)
                {
                    List<ParameterInfo> currentSet = new List<ParameterInfo>();
                    currentSet.AddRange(cmdletInfo.AllParametersParameterSet);
                    currentSet.AddRange(cmdletInfo.NamedParameterSets[setName]);

                    WriteParameterSet(setName, currentSet);
                }
            }

            _xmlWriter.WriteEndElement(); // </ParameterSets>
        }

        private void WriteParameterSet(string name, List<ParameterInfo> parameters)
        {
            parameters.Sort(ParameterComparer);

            _xmlWriter.WriteStartElement("ParameterSet");
            
            if (!string.IsNullOrEmpty(name))
            {
                _xmlWriter.WriteAttributeString("Name", name);
            }

            foreach(ParameterInfo pi in parameters)
                WriteParameter(pi);

            _xmlWriter.WriteEndElement();
        }

        private void WriteParameter(ParameterInfo paramInfo)
        {
            paramInfo.WriteTo(_xmlWriter);
        }

        private void WriteParameters(CmdletInfo cmdletInfo)
        {
            _xmlWriter.WriteStartElement("Parameters");

            List<ParameterInfo> all = new List<ParameterInfo>(cmdletInfo.GetAllParameters());
            all.Sort(ParameterComparer);

            foreach(ParameterInfo pi in all)
            {
                WriteParameter(pi);
            }

            _xmlWriter.WriteEndElement(); // </Parameters>
        }

        private void WriteXmlNodeList(IEnumerable<XmlNode> nodes, string singularTagName)
        {
            WriteList<XmlNode>(nodes, singularTagName, delegate(XmlNode xn)
            {
                xn.WriteTo(_xmlWriter);
            });
        }

        private void WriteStringList(IEnumerable<String> items, string singularTagName)
        {
            WriteList<String>(items, singularTagName, delegate(string s)
            {
                _xmlWriter.WriteStartElement(singularTagName);
                _xmlWriter.WriteString(s);
                _xmlWriter.WriteEndElement();
            });
        }

        private void WriteList<T>(IEnumerable<T> items, string singularTagName, Action<T> action)
        {
            _xmlWriter.WriteStartElement(singularTagName + 's');

            foreach (T item in items)
            {
                action(item);
            }

            _xmlWriter.WriteEndElement();
        }

        private int ParameterComparer(ParameterInfo x, ParameterInfo y)
        {
            int xp = x.Position;
            int yp = y.Position;

            if(xp < 0 && yp < 0) return 0;

            if(yp < 0) return -1;

            if(xp < 0) return 1;

            return xp.CompareTo(yp);
        }
    }
}

