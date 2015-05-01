using System;
using System.Diagnostics;
using System.Linq;
using BaseTypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.BaseTypes.Tests
{
    [TestClass]
    public class DecimalFrequencyFunctionTests
    {
        [TestMethod]
        public void ReturnsCorrectFunction()
        {
            // Arrange
            var data = new decimal[]
            {-20, -60, -10, 30, 60, 70, -10, -30, 120, -100, -80, 20, 40, -60, -10, 20, 30, -80, 60, 70};
            var freqFunc = new DecimalFrequencyFunction(data);

            // Assert
            Assert.AreEqual(freqFunc.Function.First().Y, 0.05m);
            Assert.AreEqual(freqFunc.Function.Skip(1).First().Y, 0.15m);
            Assert.AreEqual(freqFunc.Function.Last().Y, 1m);         
        }
    }
}
