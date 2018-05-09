//---------------------------------------------------------------------
// Author: jachymko
//
// Description: Gets a file version (FileVersionInfo object).
//
// Creation Date: Dec 24, 2006
//---------------------------------------------------------------------
using System;
using System.Diagnostics;
using System.IO;
using System.Management.Automation;
using Microsoft.PowerShell.Commands;
using Pscx.IO;


namespace Pscx.Commands
{
    [Cmdlet(VerbsCommon.Get, PscxNouns.FileVersionInfo, DefaultParameterSetName = ParameterSetPath)]
    [OutputType(typeof(FileVersionInfo))]
    [ProviderConstraint(typeof(FileSystemProvider))]
    public class GetFileVersionInfoCommand : PscxPathCommandBase
    {
        protected override void ProcessPath(PscxPathInfo pscxPath)
        {
            string filePath = pscxPath.ProviderPath;

            if (File.Exists(filePath))
            {
                WriteObject(FileVersionInfo.GetVersionInfo(filePath));
            }
            else if (!Directory.Exists(filePath))
            {
                ErrorHandler.WriteFileNotFoundError(filePath);
            }
        }
    }
}
