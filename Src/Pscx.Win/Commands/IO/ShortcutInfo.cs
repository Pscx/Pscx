//---------------------------------------------------------------------
// Authors: jachymko
//
// Description: Shortcut info class
//
// Creation Date: Dec 24, 2006
//---------------------------------------------------------------------

using Pscx.Win.Interop.Shell;
using System;
using System.Text;

namespace Pscx.Win.Commands.IO {
    public class ShortcutInfo {
        public string Path;
        public string Arguments;
        public string Description;
        public string TargetPath;
        public string WorkingDirectory;
        public string IconPath;
        public int IconIndex;
        public ShortcutWindowState WindowState;

        internal ShortcutInfo(string path, IShellLink native) {
            Path = path;

            StringBuilder tmp = new StringBuilder(1024);

            native.GetArguments(tmp, tmp.Capacity);
            Arguments = tmp.ToString();

            native.GetDescription(tmp, tmp.Capacity);
            Description = tmp.ToString();

            native.GetIconLocation(tmp, tmp.Capacity, out IconIndex);
            IconPath = tmp.ToString();

            native.GetPath(tmp, tmp.Capacity, IntPtr.Zero, 0);
            TargetPath = tmp.ToString();

            native.GetWorkingDirectory(tmp, tmp.Capacity);
            WorkingDirectory = tmp.ToString();

            uint ws = 0;
            native.GetShowCmd(out ws);
            WindowState = (ShortcutWindowState)(ws);
        }
    }

    public enum ShortcutWindowState {
        SW_NORMAL = 1,
        SW_SHOWMINIMIZED = 2,
        SW_SHOWMAXIMIZED = 3,
    }
}
