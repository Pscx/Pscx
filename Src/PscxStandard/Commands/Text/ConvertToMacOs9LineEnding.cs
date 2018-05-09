//---------------------------------------------------------------------
// Author: Keith Hill
//
// Description: Class to implement the ConvetTo-MacOs9LineEnding cmdlet
//              which converts all line-endings to \r.
//
// Creation Date: Nov 12, 2006
//---------------------------------------------------------------------
using System;
using System.ComponentModel;
using System.Management.Automation;

using Microsoft.PowerShell.Commands;

namespace Pscx.Commands.Text
{
    [Cmdlet(VerbsData.ConvertTo, "MacOs9LineEnding", DefaultParameterSetName = "Path", SupportsShouldProcess = true)]
    [Description("Converts the line endings in the specified file to Mac OS9 and earlier style line endings \"\\r\".")]
    [DetailedDescription("Converts the line endings in the specified file to Mac OS9 and earlier style line endings \"\\r\".  " +
                         "You can convert a single file to a new file name.  Or you can convert multiple files and " +
                         "specify a destination directory.  By default, this cmdlet will overwrite existing files unless " +
                         "you specify -NoClobber.  If you want to force the overwrite of read only files use the -Force option.  ")]
    [RelatedLink(typeof(ConvertToUnixLineEndingCommand))]
    [RelatedLink(typeof(ConvertToWindowsLineEndingCommand))]
    [ProviderConstraint(typeof(FileSystemProvider))]
    public class ConvertToMacOs9LineEndingCommand : ConvertToLineEndingBaseCommand
    {
        protected override string TargetLineEnding
        {
            get { return LineEnding.MacOs9; }
        }
    }
}