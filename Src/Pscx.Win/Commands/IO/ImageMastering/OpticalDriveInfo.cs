using System;

namespace Pscx.Win.Commands.IO.ImageMastering
{
    [Serializable]
    public struct OpticalDriveInfo
    {
        public string MountPoint;
        public string VendorId;
        public string ProductId;
        public string ProductRevision;
        public string[] SupportedProfiles;        
    }
}