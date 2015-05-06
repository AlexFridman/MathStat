using System;
using BaseTypes;
using BaseTypes.Interval;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.BaseTypes.Tests
{
    [TestClass]
    public class PiecewiseFunctionTest
    {
        [TestMethod]
        public void CalculateValueWhereFunctionIsDefined()
        {
            // Arrange
            var function = new PiecewiseFunction<int>();
            function.Functions.Add(new Interval<int>(1, 2, true), i => i + 1);
            function.Functions.Add(new Interval<int>(2, 3, false, true), i => i + 2);
            
            // Assert
            Assert.AreEqual(function.Calculate(1), 2);
            Assert.AreEqual(function.Calculate(3), 5);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CalculateValueWhereFunctionIsNotDefined()
        {
            // Arrange
            var function = new PiecewiseFunction<int>();
            function.Functions.Add(new Interval<int>(1, 2, false), i => i + 1);
            function.Functions.Add(new Interval<int>(2, 3, false, true), i => i + 2);

            // Act
            var res = function.Calculate(2);
        }
    }
}
