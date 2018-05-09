//---------------------------------------------------------------------
// Authors: AlexAngelopoulos, Keith Hill, jachymko
//
// Description: Class implementing Get-MountPoint command.
//
// Creation Date: Oct  9, 2006
// Modified Date: Dec 13, 2006
//
//---------------------------------------------------------------------
using System;
using System.ComponentModel;
using System.IO;
using System.Management.Automation;
using System.Runtime.InteropServices;
using System.Text;


using Pscx.Interop;
using Pscx.IO.Ntfs;

namespace Pscx.Commands.IO.Ntfs
{
    [Cmdlet(VerbsCommon.Get, PscxNouns.MountPoint)]
    [Description("Returns all mount points defined for a specific root path.")]
    [RelatedLink(typeof(NewHardLinkCommand)), RelatedLink(typeof(NewJunctionCommand)),
     RelatedLink(typeof(RemoveMountPointCommand)), RelatedLink(typeof(GetReparsePointCommand)), 
     RelatedLink(typeof(RemoveReparsePointCommand)), RelatedLink(typeof(NewSymlinkCommand))]
    public class GetMountPointCommand : PscxCmdlet
    {
        [AllowNull]
        [AllowEmptyString]
        [Parameter(Position = 0, HelpMessage = "When specified, gets mount points on the specified volume; otherwise, returns mount points on all volumes.")]
        public String Volume
        {
            set;
            get;
        } 

        protected override void ProcessRecord()
        {
            var volume = Path.GetPathRoot(Volume);
    
            if (String.IsNullOrEmpty(volume))
            {
                EnumerateVolumes();
            }
            else
            {
                volume = ReparsePointHelper.EnsurePathSlash(volume);
                EnumerateVolumeMountPoints(volume);
            }
        }

        private void EnumerateVolumes()
        {
            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (drive.DriveType != DriveType.Fixed) continue;

                EnumerateVolumeMountPoints(drive.RootDirectory.FullName);
            }
        }

        private void EnumerateVolumeMountPoints(String volume)
        {
            StringBuilder nameBuffer = new StringBuilder(NativeMethods.MAX_PATH);

            using (SafeFindVolumeMountPointHandle hFind = NativeMethods.FindFirstVolumeMountPoint(volume, nameBuffer, nameBuffer.Capacity))
            {
                if (hFind.IsInvalid) return;

                do
                {
                    string volumeName = string.Empty;
                    string mountPointName = Path.Combine(volume, nameBuffer.ToString());

                    if (NativeMethods.GetVolumeNameForVolumeMountPoint(mountPointName, nameBuffer, nameBuffer.Capacity))
                    {
                        volumeName = nameBuffer.ToString();
                    }

                    WriteObject(new LinkReparsePointInfo(ReparsePointType.MountPoint, mountPointName, volumeName));
                }
                while (NativeMethods.FindNextVolumeMountPoint(hFind, nameBuffer, nameBuffer.Capacity));
            }
        }
    }
}
