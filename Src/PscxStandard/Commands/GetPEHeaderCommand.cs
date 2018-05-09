//---------------------------------------------------------------------
// Author: Keith Hill
//
// Description: Class to implement the Get-PEHeader cmdlet which is 
//              used to get Portable Executable header information like
//              whether or not the file is a .NET assembly.
//
// Creation Date: Dec 18, 2006
//---------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Management.Automation;
using System.Security;
using System.Text;
using Pscx;

namespace Pscx.Commands.FileSystem
{
    [Cmdlet(VerbsCommon.Get, "PEHeader", DefaultParameterSetName = "Path"),
     Description("Gets the Portable Executable header information for an executable file (exe, dll, ocx, etc)."),
    DetailedDescription("Gets the Portable Executable header information for an executable file (exe, dll, ocx, etc).  This cmdlet emits a PEHeaderInfo object that contains header information about the executable as well as a Path property which contains the original long path.")]
    public class GetPEHeaderCommand : PSCmdlet
    {
        private string[] _paths;
        private bool _literalPathUsed;

        [Parameter(ParameterSetName = "Path",
                   HelpMessage = "Specifies the path to the file to process. Wildcard syntax is allowed.",
                   Position = 0, Mandatory = true, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true),
         AcceptsWildcards(true)]
        public string[] Path
        {
            get { return _paths; }
            set { _paths = value; }
        }

        [Alias(new string[] { "PSPath" }),
         Parameter(ParameterSetName = "LiteralPath",
                   HelpMessage = "Specifies a path to the item. The value of -LiteralPath is used exactly as it is typed. No characters are interpreted as wildcards. If the path includes escape characters, enclose it in single quotation marks. Single quotation marks tell Windows PowerShell not to interpret any characters as escape sequences.",
                   Position = 0, Mandatory = true, ValueFromPipeline = false, ValueFromPipelineByPropertyName = true),
         AcceptsWildcards(false)]
        public string[] LiteralPath
        {
            get { return _paths; }
            set
            {
                _paths = value;
                _literalPathUsed = true;
            }
        }

        protected override void ProcessRecord()
        {
            if (_paths != null)
            {
                List<string> pathList = Utils.ResolveFilePaths(this, _paths, _literalPathUsed);
                foreach (string path in pathList)
                {
                    WriteDebug(this.GetType().Name + " processing file: " + path);

                    try
                    {
                        PEHeaderInfo headerInfo = PEHeaderParser.ScanFile(path);
                        WriteObject(headerInfo);
                    }
                    catch (FileNotFoundException ex)
                    {
                        WriteError(new ErrorRecord(ex, "FileError", ErrorCategory.ObjectNotFound, path));
                    }
                    catch (SecurityException ex)
                    {
                        WriteError(new ErrorRecord(ex, "FileError", ErrorCategory.SecurityError, path));
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        WriteError(new ErrorRecord(ex, "FileError", ErrorCategory.SecurityError, path));
                    }
                    catch (Exception ex)
                    {
                        WriteError(new ErrorRecord(ex, "FileError", ErrorCategory.NotSpecified, path));
                    }
                }
            }
        }
    }
}
