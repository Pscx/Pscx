//---------------------------------------------------------------------
//
// Description: Common class to store all cmdlet nouns.
//
//---------------------------------------------------------------------

using System;

namespace Pscx
{
    internal static class PscxWinNouns {
        public const string RunningObject = "RunningObject";
        public const string Privilege = "Privilege";

        // Compression
        public const string BZip2 = "BZip2";
        public const string GZip = "GZip";
        public const string Tar = "Tar";
        public const string Zip = "Zip";
        public const string Archive = "PscxArchive";

        // NTFS
        public const string Junction = "Junction";
        public const string Hardlink = "Hardlink";
        public const string MountPoint = "MountPoint";
        public const string ReparsePoint = "ReparsePoint";
        public const string Shortcut = "Shortcut";
        public const string ShortPath = "ShortPath";
        public const string Symlink = "Symlink";
        public const string OpticalDriveInfo = "OpticalDriveInfo";
        public const string VolumeLabel = "VolumeLabel";

        public const string Apartment = "Apartment";
        public const string File = "File";
        public const string Object = "Object";
        public const string Script = "Script";
        public const string TerminalSession = "TerminalSession";
        public const string Uptime = "PscxUptime";

        // DirectoryServices
        public const string ADObject = "PscxADObject";  
        public const string DhcpServer = "DhcpServer";
        public const string DomainController = "DomainController";
        public const string UserGroupMembership = "UserGroupMembership";

        // Database
        public const string SqlData = "SqlData";
        public const string SqlDataSet = "SqlDataSet";
        public const string SqlCommand = "SqlCommand";
        public const string OleDbData = "OleDbData";
        public const string OleDbDataSet = "OleDbDataSet";
        public const string OleDbCommand = "OleDbCommand";
        public const string AdoConnection = "AdoConnection";
        public const string AdoDataProvider = "AdoDataProvider";
        public const string AdoCommand = "AdoCommand";
    }
}
