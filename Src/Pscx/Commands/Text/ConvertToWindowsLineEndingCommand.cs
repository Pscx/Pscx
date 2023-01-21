//---------------------------------------------------------------------
// Author: Keith Hill
//
// Description: Class to implement the ConvetTo-WindowsLineEnding cmdlet
//              which converts all line-endings to \r\n.
//
// Creation Date: Nov 12, 2006
//---------------------------------------------------------------------
using Microsoft.PowerShell.Commands;
using System.ComponentModel;
using System.Management.Automation;

namespace Pscx.Commands.Text
{
    [Cmdlet(VerbsData.ConvertTo, PscxNouns.WindowsLineEnding, DefaultParameterSetName = "Path", SupportsShouldProcess = true)]
    [Description("Converts the line endings in the specified file to Windows line endings \"\\r\\n\".")]
    [DetailedDescription("Converts the line endings in the specified file to Windows line endings \"\\r\\n\".  " +
                         "You can convert a single file to a new file name.  Or you can convert multiple files and " +
                         "specify a destination directory.  By default, this cmdlet will overwrite existing files unless " +
                         "you specify -NoClobber.  If you want to force the overwrite of read only files use the -Force option.  ")]
    [RelatedLink(typeof(ConvertToMacOs9LineEndingCommand))]
    [RelatedLink(typeof(ConvertToUnixLineEndingCommand))]
    [ProviderConstraint(typeof(FileSystemProvider))]
    public class ConvertToWindowsLineEndingCommand : ConvertToLineEndingBaseCommand
    {
        protected override string TargetLineEnding
        {
            get { return LineEnding.Windows; }
        }
    }
}
