//---------------------------------------------------------------------
// Author: Keith Hill
//
// Description: Class to implement the Unblock-File cmdlet.
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
    [Cmdlet(VerbsSecurity.Unblock, PscxNouns.File,
            SupportsShouldProcess = true, DefaultParameterSetName = ParameterSetPath)]
    [Description("Unblock the file so that it can be used locally.")]
    [ProviderConstraint(typeof(FileSystemProvider))]
    [RelatedLink(typeof(GetAlternateDataStreamCommand))]
    [RelatedLink(typeof(RemoveAlternateDataStreamCommand))]
    public class UnblockFileCommand : PscxInputObjectPathCommandBase
    {
        private const string _adsName = "Zone.Identifier";

        [Parameter(HelpMessage = "When specified, outputs the pipeline object passed in.")]
        public SwitchParameter PassThru { get; set; }

        protected override void OnValidateLiteralPath(IPscxPathSettings settings)
        {
            settings.PathType = PscxPathType.Leaf;
            settings.ShouldExist = true;
        }

        protected override void OnValidatePath(IPscxPathSettings settings)
        {
            settings.PathType = PscxPathType.Leaf;
            settings.ShouldExist = true;
        }

        protected override void ProcessPath(PscxPathInfo pscxPath)
        {
            try
            {
                if (FileSystem.AlternateDataStreamExists(pscxPath.ProviderPath, _adsName))
                {
                    string filename = System.IO.Path.GetFileName(pscxPath.ProviderPath);
                    if (this.ShouldProcess(filename, "unblocking file"))
                    {
                        FileSystem.DeleteAlternateDataStream(pscxPath.ProviderPath, _adsName);
                    }
                }
                else
                {
                    WriteWarning(String.Format(Properties.Resources.UnblockedAlready_F1, pscxPath.ProviderPath));
                }

                if (this.PassThru)
                {
                    WriteObject(base.InputObject);
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
