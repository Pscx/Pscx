using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Pscx.SIUnits;
using System.Globalization;

namespace PscxUnitTests.SIUnits
{
    [TestFixture]
    public class LengthTests
    {
        private static readonly CultureInfo InvariantCulture = CultureInfo.InvariantCulture;

        private static void TestLength(String expected, Double meters)
        {
            Assert.AreEqual(expected, new Length(meters).ToString(InvariantCulture));
        }

        [Test]
        public void Test00125ToString()
        {
            TestLength("1.25 cm", 0.0125);
        }

        [Test]
        public void Test001ToString()
        {
            TestLength("1 cm", 0.01);
        }

        [Test]
        public void Test01ToString()
        {
            TestLength("10 cm", 0.1);
        }

        [Test]
        public void Test0ToString()
        {
            TestLength("0 m", 0);
        }

        [Test]
        public void Test1ToString()
        {
            TestLength("1 m", 1);
        }

        [Test]
        public void Test512ToString()
        {
            TestLength("512 m", 512);
        }

        [Test]
        public void Test1000ToString()
        {
            TestLength("1 km", 1000);
        }

        [Test]
        public void Test15240ToString()
        {
            TestLength("15.24 km", 15240);
        }
    }
}