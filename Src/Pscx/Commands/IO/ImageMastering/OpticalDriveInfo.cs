using System;
using System.Collections.Generic;
using System.Text;

namespace Pscx.Commands.IO.ImageMastering
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