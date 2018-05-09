//---------------------------------------------------------------------
// Author: Keith Hill
//
// Description: Class to implement the Get-DriveInfo cmdlet.
//
// Creation Date: Feb 12, 2008
//---------------------------------------------------------------------
using System;
using System.IO;
using System.Management.Automation;
using Microsoft.PowerShell.Commands;

namespace Pscx.Commands.IO
{
    [Cmdlet(VerbsCommon.Get, PscxNouns.DriveInfo)]
    [OutputType(new[] {typeof(DriveInfo)})]
    [ProviderConstraint(typeof(FileSystemProvider))]
    public class GetDriveInfoCommand : PscxCmdlet
    {
        protected override void EndProcessing()
        {
            object target = null;
            try
            {
                DriveInfo[] drives = DriveInfo.GetDrives();
                foreach (DriveInfo driveInfo in drives)
                {
                    target = driveInfo;
                    if (driveInfo.IsReady &&
                        (driveInfo.DriveType != DriveType.Network) &&
                        (driveInfo.DriveType != DriveType.CDRom))
                    {
                        WriteObject(driveInfo);
                    }
                }
            }
            catch (PipelineStoppedException)
            {
                throw;
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "Get-DriveInfo", ErrorCategory.NotSpecified, target)); 
            }
        }
    }
}
