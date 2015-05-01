using System;
using BaseTypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.BaseTypes.Tests
{
    [TestClass]
    public class DecimalIntervalTest
    {
        [TestMethod]
        public void ReturnsCorrectMiddle()
        {
            // Arrange
            var decInterval1 = new DecimalInterval(1, 3);
            var decInterval2 = new DecimalInterval(1.5m, 3);

            // Assert
            Assert.AreEqual(decInterval1.Middle, 2);
            Assert.AreEqual(decInterval2.Middle, 2.25m);
        }

        [TestMethod]
        public void ReturnsCorrectLength()
        {
            // Arrange
            var decInterval = new DecimalInterval(1, 3);

            // Assert
            Assert.AreEqual(decInterval.Length, 2);
        }
    }
}
