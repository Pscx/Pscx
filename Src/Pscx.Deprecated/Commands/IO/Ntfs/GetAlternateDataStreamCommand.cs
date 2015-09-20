//---------------------------------------------------------------------
// Author: Keith Hill
//
// Description: Class to implement the Get-AlternateDataStream cmdlet.
//
// Creation Date: March 7, 2010
//---------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Management.Automation;
using System.Security;
using Microsoft.PowerShell.Commands;
using Pscx.Commands;
using Pscx.IO;
using Trinet.Core.IO.Ntfs;

namespace Pscx.Deprecated.Commands.IO.Ntfs
{
    [Cmdlet(VerbsCommon.Get, PscxNouns.AlternateDataStream, DefaultParameterSetName = ParameterSetPath)]
    [OutputType(new[] { typeof(AlternateDataStreamInfo) })]
    [Description("Gets the alternate data streams in an NTFS file.")]
    [DetailedDescription("Gets the alternate data streams in an NTFS file.  Use the List parameter to list all the alternate data streams.")]
    [ProviderConstraint(typeof(FileSystemProvider))]
    [RelatedLink(typeof(RemoveAlternateDataStreamCommand))]
    [RelatedLink(typeof(UnblockFileCommand))]
    public class GetAlternateDataStreamCommand : PscxPathCommandBase
    {
        [Parameter(Position = 1, 
                   HelpMessage = "Specifies the name of the alternate data stream e.g. Zone.Identifier")]
        public string[] Name { get; set; }

        [Parameter]
        public SwitchParameter List { get; set; }

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
                if (List)
                {
                    IList<AlternateDataStreamInfo> streamInfos = FileSystem.ListAlternateDataStreams(pscxPath.ProviderPath);
                    WriteObject(streamInfos, true);
                }
                else
                {
                    foreach (string aName in Name)
                    {
                        if (!FileSystem.AlternateDataStreamExists(pscxPath.ProviderPath, aName))
                        {
                            this.ErrorHandler.WriteAlternateDataStreamDoentExist(aName, pscxPath.ProviderPath);
                            continue;
                        }
                        AlternateDataStreamInfo streamInfo = FileSystem.GetAlternateDataStream(pscxPath.ProviderPath, aName);
                        WriteObject(streamInfo);
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
