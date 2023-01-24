//---------------------------------------------------------------------
// Author: Keith Hill
//
// Description: Class to implement the Test-Script cmdlet.
//
// Creation Date: Sept 27, 2009
//---------------------------------------------------------------------
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Management.Automation;
using System.Text;
using Microsoft.PowerShell.Commands;
using Pscx.Core.IO;
using System.ComponentModel;

namespace Pscx.Commands
{
    [OutputType(typeof(bool))]
    [Cmdlet(VerbsDiagnostic.Test, PscxNouns.Script, DefaultParameterSetName = ParameterSetPath), Description("Test script for validity")]
    [OutputType(new[]{typeof(bool)})]
    [ProviderConstraint(typeof(FileSystemProvider))]
    public class TestScriptCommand : PscxInputObjectPathCommandBase
    {
        [Parameter]
        [ValidateCount(1, 2)]
        public new int[] Context { get; set; }

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
            RegisterInputType<string>(str => TestScript(str, null));

            // Dont throw on directories, just ignore them
            IgnoreInputType<DirectoryInfo>();

            base.BeginProcessing();
        }

        protected override void ProcessPath(PscxPathInfo pscxPath)
        {
            FileHandler.ProcessRead(pscxPath.ProviderPath, delegate(Stream stream)
            {
                using (var streamReader = new StreamReader(stream))
                {
                    string script = streamReader.ReadToEnd();
                    TestScript(script, pscxPath.ToPathInfo().Path);
                }
            });
        }

        private void TestScript(string script, string path)
        {
            Collection<PSParseError> parseErrors;
            PSParser.Tokenize(script, out parseErrors);
            bool hasErrors = parseErrors.Count > 0;
            if (hasErrors)
            {
                foreach (var parseError in parseErrors)
                {
                    var strBld = new StringBuilder();

                    var errorMessage = 
                        String.Format("Parse error on line:{0} char:{1} - {2}",
                                      parseError.Token.StartLine, parseError.Token.StartColumn,
                                      parseError.Message);
                    strBld.AppendLine(errorMessage);
                    if (Context != null)
                    {
                        string[] lines = script.Split('\n');
                        for (int i = 0; i < lines.Length; i++)
                        {
                            lines[i] = lines[i].TrimEnd();
                        }

                        int startLine = parseError.Token.StartLine;
                        int start = Math.Max(1, startLine - Context[0]);
                        int numLinesAfter = (Context.Length == 2) ? Context[1] : Context[0];
                        int endLine = Math.Min(lines.Length, startLine + numLinesAfter);
                        
                        // Create format message
                        string filename = "";
                        if (!String.IsNullOrEmpty(path))
                        {
                            filename = System.IO.Path.GetFileName(path) + ":";
                        }
                        string formatMsg = String.Format("{{0,1}} {0}{{1}}: {{2}}", filename);

                        // Display context lines before erroring line
                        for (int i = start; i < startLine; i++)
                        {
                            strBld.AppendFormat(formatMsg, "", i, lines[i-1]);
                            strBld.AppendLine();
                        }

                        // Display erroring line
                        string badLine = lines[startLine - 1];
                        int startCol = Math.Max(1, parseError.Token.StartColumn);
                        startCol = Math.Min(badLine.Length, startCol);
                        badLine = badLine.Insert(startCol - 1, "<<<< ");
                        strBld.AppendFormat(formatMsg, ">", startLine, badLine);
                        strBld.AppendLine();

                        // Display context lines after erroring line
                        for (int i = startLine + 1; i <= endLine; i++)
                        {
                            strBld.AppendFormat(formatMsg, "", i, lines[i - 1]);
                            strBld.AppendLine();
                        }
                    }

                    WriteWarning(strBld.ToString());
                }
            }

            WriteObject(!hasErrors);
        }
    }
}
