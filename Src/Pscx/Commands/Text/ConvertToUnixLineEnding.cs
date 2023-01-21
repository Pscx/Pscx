//---------------------------------------------------------------------
// Author: Keith Hill
//
// Description: Class to implement the ConvetTo-UnixLineEnding cmdlet
//              which converts all line-endings to \n.
//
// Creation Date: Nov 12, 2006
//---------------------------------------------------------------------
using Microsoft.PowerShell.Commands;
using System.ComponentModel;
using System.Management.Automation;

namespace Pscx.Commands.Text
{
    [Cmdlet(VerbsData.ConvertTo, PscxNouns.UnixLineEnding, DefaultParameterSetName = "Path", SupportsShouldProcess = true)]
    [Description("Converts the line endings in the specified file to Unix line endings \"\\n\".")]
    [DetailedDescription("Converts the line endings in the specified file to Unix line endings \"\\n\".  " +
                         "You can convert a single file to a new file name.  Or you can convert multiple files and " +
                         "specify a destination directory.  By default, this cmdlet will overwrite existing files unless " +
                         "you specify -NoClobber.  If you want to force the overwrite of read only files use the -Force option.  ")]
    [RelatedLink(typeof(ConvertToMacOs9LineEndingCommand))]
    [RelatedLink(typeof(ConvertToWindowsLineEndingCommand))]
    [ProviderConstraint(typeof(FileSystemProvider))]
    public class ConvertToUnixLineEndingCommand : ConvertToLineEndingBaseCommand
    {
        protected override string TargetLineEnding
        {
            get { return LineEnding.Unix; }
        }
    }
}