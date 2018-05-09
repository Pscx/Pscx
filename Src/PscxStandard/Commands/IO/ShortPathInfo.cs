//---------------------------------------------------------------------
// Author: Keith Hill
//
// Description: Class to represent info returned by Get-ShortPath cmdlet.
//
// Creation Date: Dec 9, 2006
//---------------------------------------------------------------------

namespace Pscx.Commands.IO
{
    public class ShortPathInfo
    {
        public ShortPathInfo(string path, string shortPath)
        {
            this.Path = path;
            this.ShortPath = shortPath;
        }

        public string ShortPath { get; private set; }
        public string Path { get; private set; }

        public override string ToString()
        {
            return this.ShortPath;
        }
    }
}
