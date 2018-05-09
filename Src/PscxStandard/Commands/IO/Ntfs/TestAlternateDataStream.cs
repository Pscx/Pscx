//---------------------------------------------------------------------
// Author: Keith Hill
//
// Description: Class to implement the Test-AlternateDataStream cmdlet.
//
// Creation Date: March 7, 2010
//---------------------------------------------------------------------
using System;
using System.ComponentModel;
using System.Management.Automation;
using System.Security;
using Microsoft.PowerShell.Commands;
using Pscx.IO;
using Trinet.Core.IO.Ntfs;

namespace Pscx.Commands.IO.Ntfs
{
    [Cmdlet(VerbsDiagnostic.Test, PscxNouns.AlternateDataStream,
            DefaultParameterSetName = ParameterSetPath)]
    [OutputType(new[] {typeof(bool)})]
    [Description("Tests for the existence of the specified alternate data stream from an NTFS file.")]
    [ProviderConstraint(typeof(FileSystemProvider))]
    public class TestAlternateDataStreamCommand : PscxPathCommandBase
    {
        [Parameter(Mandatory = true, Position = 1,
                   HelpMessage = "Name of the alternate data stream e.g. Zone.Identifier")]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

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
                WriteVerbose("Processing " + pscxPath.ProviderPath);
                bool exists = FileSystem.AlternateDataStreamExists(pscxPath.ProviderPath, Name);
                WriteObject(exists);
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
