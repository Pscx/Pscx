using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Management.Automation;
using NUnit.Framework;

namespace PscxUnitTests.Xml
{
    [TestFixture]
    class TestXmlTests : PscxCmdletTest
    {
        [Test]
        public void SchemaTest_GoodXml()
        {
            string xmlPath = Path.Combine(this.ProjectDir, @"Xml\passes_schema_validation.xml");
            string schemaPath = Path.Combine(this.ProjectDir, @"Xml\test.xsd");

            string script = String.Format("Test-Xml '{0}' -SchemaPath '{1}'", xmlPath, schemaPath);
            Collection<PSObject> results = this.Invoke(script);

            Assert.AreEqual(1, results.Count);
            bool result = (bool)results[0].BaseObject;
            Assert.IsTrue(result);
        }

        [Test]
        public void SchemaTest_BadXml()
        {
            string xmlPath = Path.Combine(this.ProjectDir, @"Xml\fails_schema_validation.xml");
            string schemaPath = Path.Combine(this.ProjectDir, @"Xml\test.xsd");

            string script = String.Format("Test-Xml '{0}' -SchemaPath '{1}'", xmlPath, schemaPath);
            Collection<PSObject> results = this.Invoke(script);

            Assert.AreEqual(1, results.Count);
            bool result = (bool)results[0].BaseObject;
            Assert.IsFalse(result);
        }
    }
}
