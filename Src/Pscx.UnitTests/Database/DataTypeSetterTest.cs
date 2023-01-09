//---------------------------------------------------------------------
// Author: Harley Green
//
// Description: Cmdlet to get data from Sql Server databases
//
// Creation Date: 2008/3/8
//---------------------------------------------------------------------
using System;
using System.Data;
using NUnit.Framework;
using Pscx.Win.Reflection.DynamicType;

namespace PscxUnitTests.Database
{
    [TestFixture]
    public class DataTypeSetterTest
    {
        public class TestClass
        {
            public int? I { get; set; }

            public string S { get; set; }
        }

        [Test]
        public void SetValues_SetsProperties()
        {
            var table = new DataTable();
            table.Columns.Add(new DataColumn("I", typeof (int)));
            table.Columns.Add(new DataColumn("S", typeof (string)));
            DataRow row = table.Rows.Add(10, "Value");

            var setter = new PropertySetter(typeof(TestClass));
            var testClass = new TestClass();
            setter.SetValues(testClass, new DataRowIndexer(row), false);
            Assert.AreEqual(10, testClass.I);
            Assert.AreEqual("Value", testClass.S);
        }

        [Test]
        public void SetValues_SetsDBNull()
        {
            var table = new DataTable();
            table.Columns.Add(new DataColumn("I", typeof (int)));
            table.Columns.Add(new DataColumn("S", typeof (string)));
            DataRow row = table.Rows.Add(DBNull.Value, "Value");

            var setter = new PropertySetter(typeof(TestClass));
            var testClass = new TestClass();
            setter.SetValues(testClass, new DataRowIndexer(row), false);
            Assert.AreEqual(null, testClass.I);
            Assert.AreEqual("Value", testClass.S);
        }
    }
}