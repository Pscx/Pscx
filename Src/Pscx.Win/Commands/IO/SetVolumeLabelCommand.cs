using System.ComponentModel;
using System.Management.Automation;
using System.Runtime.InteropServices;

namespace Pscx.Commands.IO
{
    /// <summary>
    /// Sets the label for a specific volume.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, PscxWinNouns.VolumeLabel), 
     Description("Modifies the label shown in Windows Explorer for a particular disk volume.")]
    [OutputType(new[]{typeof(bool)})]
    public class SetVolumeLabel :PSCmdlet
    {
        private string _path = string.Empty;
        private string _label = string.Empty;

        [Parameter(Position = 0, ValueFromPipelineByPropertyName = true,
                   HelpMessage = "Path to the root directory of a file system volume. This can be a folder where a volume is mounted. If not specified, defaults to the root of the current FileSystem location.")]
        public string Path
        {
            set { _path = value; }
            get { return _path; }
        } 

        [ValidateLength(0, 32),
         Parameter(Position = 1, ValueFromPipelineByPropertyName = true, 
                   HelpMessage ="New volume label. If not specified, the volume label will be blank.")]
        public string Label
        {
            set { _label = value; }
            get { return _label; }
        } 

        [DllImport("kernel32", SetLastError = true, EntryPoint = "SetVolumeLabel")]
        static extern uint __SetVolumeLabel(string VolumeRootPath, string NewVolumeLabel);

        protected override void ProcessRecord()
        {
            WriteVerbose("Path is " + this._path);
            WriteVerbose("Name is " + this._label);
            string volumePath = GetCanonicalVolumePath(this._path);
            uint result = __SetVolumeLabel(volumePath, this._label);
            if (result == 0)
            {
                WriteObject(false);
            }
            else
            {
                WriteObject(true);
            }
        }

        /// <summary>
        /// Transforms path argument into a path acceptable by SetVolumeLabel
        /// API call.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private string GetCanonicalVolumePath(string path)
        {
            string volumePath;
            if (path.Length == 0)
            {
                // the SetVolumeLabel API function defaults to using the current directory's root volume if no path is specified. We do NOT do that since the correct analogue in PowerShell is Location.
                WriteVerbose("empty path supplied, using FileSystem provider location.");
                volumePath = System.IO.Path.GetPathRoot(this.CurrentProviderLocation("FileSystem").Path);
                WriteVerbose("Root for current FileSystem location is " + volumePath);
            }
            else if(path.Length == 1)
            {
                // This is ok if it is a "bare" drive letter.
                // We assume that it is and terminate it as required.
                volumePath = path
                    + System.IO.Path.VolumeSeparatorChar
                    + System.IO.Path.DirectorySeparatorChar;
            }
            else if (path.Substring(path.Length - 1, 1) != System.IO.Path.DirectorySeparatorChar.ToString())
            {
                volumePath = path + System.IO.Path.DirectorySeparatorChar;
            }
            else
            {
                // this path is in the right form; treat it as a volume root.
                volumePath = path;
            }
            WriteVerbose("Canonical volume path is " + volumePath);
            return volumePath;
        }
    }

}

