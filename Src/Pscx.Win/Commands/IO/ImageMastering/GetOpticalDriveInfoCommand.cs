using Pscx.Win.Interop;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Management.Automation;
using System.Reflection;
using System.Runtime.Versioning;

namespace Pscx.Win.Commands.IO.ImageMastering {
    /// <summary>
    /// 
    /// </summary>
    [Cmdlet(VerbsCommon.Get, PscxWinNouns.OpticalDriveInfo), Description("Lists Optical drive information")]
    [SupportedOSPlatform("windows")]
    public class GetOpticalDriveInfoCommand : Imapi2CommandBase {
        /// <summary>
        /// 
        /// </summary>
        [Parameter(Position = 0)]
        [ValidateNotNullOrEmpty]
        public string[] DriveLetter {
            get;
            set;
        }

        protected override void EndProcessing() {
            try {
                List<OpticalDriveInfo> drives = GetOpticalDriveInfo();

                foreach (var drive in drives) {
                    // Avoid "access to modified closure" warning from R#
                    // not a big deal since the delegate is executed before the 
                    // next iteration, but R# doesn't know this.
                    OpticalDriveInfo tempDrive = drive;

                    // drive.MountPoint is in format "D:\" and DriveLetter(s) passed
                    // might be D, D:, d: or D:\ (for example), so only compare 
                    // first letter of each string.
                    if ((DriveLetter == null) || Array.Exists(
                                                     DriveLetter, mountPoint => String.Compare(
                                                                                    tempDrive.MountPoint, 0, mountPoint, 0, 1,
                                                                                    StringComparison.OrdinalIgnoreCase) == 0)) {
                        WriteObject(drive);
                    }
                }
            } catch (PipelineStoppedException) {
                throw;
            } catch (Exception ex) {
                WriteWarning(ex.ToString());
                this.ErrorHandler.ThrowPlatformNotSupported("IMAPI2 Interfaces not found.");
            }
        }

        private static List<OpticalDriveInfo> GetOpticalDriveInfo() {
            var drives = new List<OpticalDriveInfo>();

            var discMasterType = Type.GetTypeFromProgID(
                PROGID_IMAPI2_DISC_MASTER2, true);

            var discRecorderType = Type.GetTypeFromProgID(
                PROGID_IMAPI2_DISC_RECORDER2, true);


            var flags = BindingFlags.Instance | BindingFlags.Public;

            using (var discMaster = new SimpleComWrapper(discMasterType)) {
                foreach (var identifier in ((IEnumerable)discMaster.ComInstance)) {
                    using (var discRecorder = new SimpleComWrapper(discRecorderType)) {

                        // oh dear god, hurry up c# 4.0!
                        discRecorderType.InvokeMember(
                            "InitializeDiscRecorder",
                            flags | BindingFlags.InvokeMethod,
                            null, discRecorder.ComInstance, new[] { identifier });

                        string mountPoint = String.Empty;
                        foreach (var volumePathName in discRecorder.GetPropertyValue<IEnumerable>("VolumePathNames")) {
                            mountPoint = (string)volumePathName;
                            break;
                        }

                        drives.Add(new OpticalDriveInfo {
                            MountPoint = mountPoint,
                            VendorId = discRecorder.GetPropertyValue<string>("VendorId"),
                            ProductId = discRecorder.GetPropertyValue<string>("ProductId"),
                            ProductRevision = discRecorder.GetPropertyValue<string>("ProductRevision"),
                            SupportedProfiles =
                                               GetSupportedProfiles(
                                               discRecorder.GetPropertyValue<IEnumerable>("SupportedProfiles"))
                        });
                    }
                }
            }

            return drives;
        }

        private static string[] GetSupportedProfiles(IEnumerable profiles) {
            var profilesNames = new List<string>();
            foreach (int profile in profiles) {
                profilesNames.Add(ImapiProfileTypeMappings.GetProfileName(profile));
            }

            return profilesNames.ToArray();
        }
    }
}