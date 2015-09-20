using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Management.Automation;
using NUnit.Framework;

namespace PscxUnitTests.IO
{
    [TestFixture]
    class PscxPathInfoTests : PscxCmdletTest
    {
        [Test]
        public void PathParameter_Single()
        {
            string pattern = "Pscx.Format.ps1xml";
            string formatDataDir = Path.Combine(this.SolutionDir, @"Pscx\Bin\" + this.Configuration + @"\FormatData");

            string script = String.Format("Set-Location '{0}'; Test-Xml {1}", formatDataDir, pattern);
            Collection<PSObject> results = this.Invoke(script);

            VerifyTestXmlOnFormatDataResults(formatDataDir, pattern, results);
        }

        [Test]
        public void PathParameter_Array()
        {
            string pattern = "Pscx.Format.ps1xml,Pscx.Archive.Format.ps1xml";
            string formatDataDir = Path.Combine(this.SolutionDir, @"Pscx\Bin\" + this.Configuration + @"\FormatData");

            string script = String.Format("Set-Location '{0}'; Test-Xml {1}", formatDataDir, pattern);
            Collection<PSObject> results = this.Invoke(script);

            VerifyTestXmlOnFormatDataResults(2, results);
        }

        [Test]
        public void PathParameter_Wildcarded()
        {
            string ext = "*.ps1xml";
            string formatDataDir = Path.Combine(this.SolutionDir, @"Pscx\Bin\" + this.Configuration + @"\FormatData");

            string script = String.Format("Set-Location '{0}'; Test-Xml {1}", formatDataDir, ext);
            Collection<PSObject> results = this.Invoke(script);

            VerifyTestXmlOnFormatDataResults(formatDataDir, ext, results);
        }

        [Test]
        public void LiteralPathParameter_Single()
        {
            string pattern = "Pscx.Format.ps1xml";
            string formatDataDir = Path.Combine(this.SolutionDir, @"Pscx\Bin\" + this.Configuration + @"\FormatData");

            string script = String.Format("Set-Location '{0}'; Test-Xml -LiteralPath {1}", formatDataDir, pattern);
            Collection<PSObject> results = this.Invoke(script);

            VerifyTestXmlOnFormatDataResults(formatDataDir, pattern, results);
        }

        [Test]
        public void LiteralPathParameter_Array()
        {
            string pattern = "Pscx.Format.ps1xml,Pscx.Archive.Format.ps1xml";
            string formatDataDir = Path.Combine(this.SolutionDir, @"Pscx\Bin\" + this.Configuration + @"\FormatData");

            string script = String.Format("Set-Location '{0}'; Test-Xml -LiteralPath {1}", formatDataDir, pattern);
            Collection<PSObject> results = this.Invoke(script);

            VerifyTestXmlOnFormatDataResults(2, results);
        }

        [Test]
        public void LiteralPathParameter_NoWildcard()
        {
            //string pattern = "Pscx.Format.ps1xml,Pscx.Archive.Format.ps1xml";
            string formatDataDir = Path.Combine(this.SolutionDir, @"Pscx\Bin\" + this.Configuration + @"\FormatData");
            string srcFile = Path.Combine(formatDataDir, "Pscx.Format.ps1xml");
            string dstFile1 = Path.Combine(formatDataDir, "Pscx.Gormat.ps1xml");
            string dstFile2 = Path.Combine(formatDataDir, "Pscx.[a-z]ormat.ps1xml");
            File.Copy(srcFile, dstFile1, true);
            File.Copy(srcFile, dstFile2, true);

            try
            {
                string script = String.Format("Set-Location '{0}'; Test-Xml -LiteralPath {1}", formatDataDir, dstFile2);
                Collection<PSObject> results = this.Invoke(script);

                VerifyTestXmlOnFormatDataResults(1, results);
            }
            finally
            {
                if (File.Exists(dstFile1)) File.Delete(dstFile1);
                if (File.Exists(dstFile2)) File.Delete(dstFile2);
            }
        }

        [Test]
        public void PipelineInputByPropertyName_PSPath_SpecifiedParentDir()
        {
            string ext = "*.ps1xml";
            string pscxDir = Path.Combine(this.SolutionDir, @"Pscx\Bin\" + this.Configuration);

            string script = String.Format("Get-ChildItem {0} -r {1} | Test-Xml", pscxDir, ext);
            Collection<PSObject> results = this.Invoke(script);

            VerifyTestXmlOnFormatDataResults(pscxDir, ext, results);
        }

        [Test]
        public void PipelineInputByPropertyName_PSPath_RelPath()
        {
            string ext = "*.ps1xml";
            string pscxDir = Path.Combine(this.SolutionDir, @"Pscx\Bin\" + this.Configuration);

            string script = String.Format("Set-Location '{0}';Get-ChildItem . -r {1} | Test-Xml", pscxDir, ext);
            Collection<PSObject> results = this.Invoke(script);

            VerifyTestXmlOnFormatDataResults(pscxDir, ext, results);
        }

        [Test]
        public void PipelineInputByPropertyName_PSPath_SpecifiedParentDirForeach()
        {
            string ext = "*.ps1xml";
            string pscxDir = Path.Combine(this.SolutionDir, @"Pscx\Bin\" + this.Configuration);

            string script = String.Format("Get-ChildItem {0} -r {1} | Foreach {{Test-Xml $_}}", pscxDir, ext);
            Collection<PSObject> results = this.Invoke(script);

            VerifyTestXmlOnFormatDataResults(pscxDir, ext, results);
        }

        [Test]
        public void PipelineInputByPropertyName_PSPath_RelPathForeach()
        {
            string ext = "*.ps1xml";
            string pscxDir = Path.Combine(this.SolutionDir, @"Pscx\Bin\" + this.Configuration);

            string script = String.Format("Set-Location '{0}';Get-ChildItem . -r {1} | Foreach {{Test-Xml $_}}", pscxDir, ext);
            Collection<PSObject> results = this.Invoke(script);

            VerifyTestXmlOnFormatDataResults(pscxDir, ext, results);
        }

        [Test]
        public void PipelineInputByPropertyName_Path_AbsolutePath()
        {
            string formatDataFile = Path.Combine(this.SolutionDir, @"Pscx\Bin\" + this.Configuration + @"\FormatData\Pscx.Format.ps1xml");

            string script = String.Format("new-object psobject -prop @{{Path = '{0}'}} | Test-AlternateDataStream -name foo", formatDataFile);
            Collection<PSObject> results = this.Invoke(script);

            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("False", results[0].ToString());
        }

        [Test]
        public void PipelineInputByPropertyName_LiteralPath_AbsolutePath()
        {
            string formatDataFile = Path.Combine(this.SolutionDir, @"Pscx\Bin\" + this.Configuration + @"\FormatData\Pscx.Format.ps1xml");

            string script = String.Format("new-object psobject -prop @{{LiteralPath = '{0}'}} | Test-AlternateDataStream -name foo", formatDataFile);
            Collection<PSObject> results = this.Invoke(script);

            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("False", results[0].ToString());
        }

        private void VerifyTestXmlOnFormatDataResults(string path, string ext, Collection<PSObject> results)
        {
            string[] files = Directory.GetFiles(path, ext, SearchOption.AllDirectories);

            Assert.AreEqual(files.Length, results.Count);
            VerifyTrueResult(results);
        }

        private void VerifyTestXmlOnFormatDataResults(int numResultsExpected, Collection<PSObject> results)
        {
            Assert.AreEqual(numResultsExpected, results.Count);
            VerifyTrueResult(results);
        }

        private void VerifyTrueResult(Collection<PSObject> results)
        {
            foreach (var psobj in results)
            {
                var str = psobj.ToString();
                if (str != "True")
                {
                    Assert.Fail("Invalid result " + str);
                }
            }
        }
    }
}
