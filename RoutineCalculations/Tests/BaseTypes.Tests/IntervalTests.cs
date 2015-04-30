using System;
using BaseTypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.BaseTypes.Tests
{
    [TestClass]
    public class IntervalTests
    {
        [TestMethod]
        public void OpenIntervalContainsTest()
        {
            // Arrange
            var interval = new Interval<int>(0, 4, false, false);            

            // Assert
            Assert.AreEqual(interval.Contains(3), true);
            Assert.AreEqual(interval.Contains(5), false);
        }

        [TestMethod]
        public void LeftClosedIntervalContainsTest()
        {
            // Arrange
            var interval = new Interval<int>(0, 4, true, false);

            // Assert
            Assert.AreEqual(interval.Contains(0), true);
            Assert.AreEqual(interval.Contains(2), true);
            Assert.AreEqual(interval.Contains(-1), false);
        }

        [TestMethod]
        public void RigthClosedIntervalContainsTest()
        {
            // Arrange
            var interval = new Interval<int>(0, 4, false, true);

            // Assert
            Assert.AreEqual(interval.Contains(4), true);
            Assert.AreEqual(interval.Contains(2), true);
            Assert.AreEqual(interval.Contains(5), false);
        }


        [TestMethod]
        public void ClosedIntervalContainsTest()
        {
            // Arrange
            var interval = new Interval<int>(0, 4, true, true);

            // Assert
            Assert.AreEqual(interval.Contains(2), true);
            Assert.AreEqual(interval.Contains(0), true);
            Assert.AreEqual(interval.Contains(4), true);
            Assert.AreEqual(interval.Contains(-1), false);
            Assert.AreEqual(interval.Contains(5), false);
        }

    }
}
