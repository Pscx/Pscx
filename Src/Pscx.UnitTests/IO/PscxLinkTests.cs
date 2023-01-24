using NUnit.Framework;
using Pscx;
using Pscx.Core;
using Pscx.Win.Interop;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Management.Automation;

namespace PscxUnitTests.IO {
    [TestFixture]
    internal class PscxLinkTests : PscxCmdletTest {
        [Test]
        public void NewSymlink_1() {
            //string pattern = "Pscx.Format.ps1xml,Pscx.Archive.Format.ps1xml";
            string dataDir = Path.Combine(SolutionDir, @"Pscx\Bin\" + Configuration + @"\FormatData");
            string lnkName = "ApppLink";
            string targetDir = Path.GetFullPath(Path.Combine(dataDir, "../Apps"));

            try {
                string script = string.Format("Set-Location '{0}'; New-Symlink -Path {1} -Target {2}", dataDir, lnkName, targetDir);
                Collection<PSObject> results = Invoke(script);

                VerifyLinkOnResults(1, results);
                VerifyLink(dataDir, lnkName, targetDir, results[0]);
            } finally {
                RemoveLink(dataDir, lnkName);
            }
        }

        protected void VerifyLinkOnResults(int numExpectedResults, Collection<PSObject> results) {
            Assert.AreEqual(numExpectedResults, results.Count);
            DirectoryInfo di = (DirectoryInfo)results[0].BaseObject;
            Assert.True((di.Attributes & FileAttributes.ReparsePoint) != 0);
        }

        protected void VerifyLink(string folder, string linkName, string target, PSObject dir) {
            DirectoryInfo di = (DirectoryInfo) dir.BaseObject;
            Assert.AreEqual(linkName, di.Name);
            StringAssert.AreEqualIgnoringCase(folder, di.Parent.FullName);
            Assert.AreEqual(target, dir.Properties["Target"].Value);
        }

        protected void RemoveLink(string folder, string linkName) {
            string path = Path.Combine(folder, linkName);
            FileSystemInfo fi = Utils.GetFileOrDirectory(path);
            if (fi is DirectoryInfo) {
                NativeMethods.RemoveDirectory(path);
            } else if (fi is FileInfo) {
                NativeMethods.DeleteFile(path);
            }
        }
    }
}