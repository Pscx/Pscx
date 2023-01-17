//---------------------------------------------------------------------
// Authors: jachymko
//
// Description: Classes describing reparse points and symbolic links.
//
// Creation Date: Dec 15, 2006
//---------------------------------------------------------------------

using Pscx.Win.Interop;

namespace Pscx.Win.Fwk.IO.Ntfs {
    public class ReparsePointInfo {
        readonly string path;
        readonly ReparsePointType tag;

        protected internal ReparsePointInfo(string path, ReparsePointType tag) {
            this.path = path;
            this.tag = tag;
        }

        public string Path {
            get { return path; }
        }

        public ReparsePointType ReparsePointTag {
            get { return tag; }
        }
    }

    public class LinkReparsePointInfo : ReparsePointInfo {
        readonly string target;

        public LinkReparsePointInfo(ReparsePointType type, string path, string target)
            : base(path, type) {
            this.target = ReparsePointHelper.MakeParsedPath(target);
        }

        public string Target {
            get { return target; }
        }
    }
}

