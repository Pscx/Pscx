using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace PscxUnitTests.Accelerators {
    [TestFixture]
    internal class Base64Test : PscxCmdletTest {
        [Test]
        public void FileContentTest() {
            string dataDir = Path.Combine("D:\\");
            string fileName = "./ssh.png";

            string script = string.Format("Set-Location '{0}'; [base64](gc {1})", dataDir, fileName);
            Collection<PSObject> results = Invoke(script);

            Assert.True(results.Count == 1);
            Console.Write(results[0]);

        }
    }
}
