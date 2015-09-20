//---------------------------------------------------------------------
// Author: Keith Hill
//
// Description: Class to implement the Remove-AlternateDataStream cmdlet.
//
// Creation Date: March 7, 2010
//---------------------------------------------------------------------
using System;
using System.ComponentModel;
using System.Management.Automation;
using System.Security;
using Microsoft.PowerShell.Commands;
using Pscx.Commands;
using Pscx.IO;
using Trinet.Core.IO.Ntfs;

namespace Pscx.Deprecated.Commands.IO.Ntfs
{
    [Cmdlet(VerbsCommon.Remove, PscxNouns.AlternateDataStream, 
            SupportsShouldProcess = true, DefaultParameterSetName = ParameterSetPath)]
    [Description("Removes the specified alternate data streams from an NTFS file.")]
    [ProviderConstraint(typeof(FileSystemProvider))]
    [RelatedLink(typeof(GetAlternateDataStreamCommand))]
    [RelatedLink(typeof(UnblockFileCommand))]
    public  class RemoveAlternateDataStreamCommand : PscxPathCommandBase
    {
        [Parameter(Mandatory = true, Position = 1,
                   HelpMessage = "Name of the alternate data stream e.g. Zone.Identifier")]
        [ValidateNotNullOrEmpty]
        public string[] Name { get; set; }

        protected override void OnValidatePath(IPscxPathSettings settings)
        {
            settings.PathType = PscxPathType.Leaf;
            settings.ShouldExist = true;
        }

        protected override void ProcessPath(PscxPathInfo pscxPath)
        {
            try
            {
                foreach (string aName in Name)
                {
                    if (!FileSystem.AlternateDataStreamExists(pscxPath.ProviderPath, aName))
                    {
                        this.ErrorHandler.WriteAlternateDataStreamDoentExist(aName, pscxPath.ProviderPath);
                        continue;
                    }
                    string filename = System.IO.Path.GetFileName(pscxPath.ProviderPath);
                    if (this.ShouldProcess(filename, "removing alternate data stream " + aName))
                    {
                        FileSystem.DeleteAlternateDataStream(pscxPath.ProviderPath, aName);
                    }
                }
            }
            catch (SecurityException ex)
            {
                WriteError(new ErrorRecord(ex, "FileError", ErrorCategory.SecurityError, pscxPath.ProviderPath));
            }
            catch (UnauthorizedAccessException ex)
            {
                WriteError(new ErrorRecord(ex, "FileError", ErrorCategory.SecurityError, pscxPath.ProviderPath));
            }
            catch (PipelineStoppedException)
            {
                throw;
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "FileError", ErrorCategory.NotSpecified, pscxPath.ProviderPath));
            }
        }
    }
}
