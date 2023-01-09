//---------------------------------------------------------------------
// Author: Keith Hill
//
// Description: Class to implement the Get-ShortPath cmdlet which is 
//              used to get the short names for the components of a 
//              path.
//
// Creation Date: Nov 23, 2006
//---------------------------------------------------------------------

using Microsoft.PowerShell.Commands;
using Pscx.Commands;
using Pscx.Core.IO;
using System.ComponentModel;
using System.IO;
using System.Management.Automation;

namespace Pscx.Win.Commands.IO
{
    [Cmdlet(VerbsCommon.Get, PscxWinNouns.ShortPath, DefaultParameterSetName = "Path"),
     Description("Gets the short, 8.3 name for the given path."),
     DetailedDescription("Gets the short, 8.3 name for the given path.  This cmdlet emits a ShortPathInfo object that contains a ShortPath property as well as a Path property which contains the original long path.")]
    [OutputType(new[] {typeof(string)})]
    [ProviderConstraint(typeof(FileSystemProvider))]
    public class GetShortPathCommand : PscxPathCommandBase
    {
        protected override void ProcessPath(PscxPathInfo pscxPath)
        {
            string filePath = pscxPath.ProviderPath;
            try
            {
                string shortPath = Utils.GetShortPathName(filePath);
                var shortPathInfo = new ShortPathInfo(filePath, shortPath);
                WriteObject(shortPathInfo);
            }
            catch (IOException exc)
            {
                ErrorHandler.HandleFileError(false, filePath, exc);
            }
        }
    }
}

