using System;
using System.Linq;
using BaseTypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.BaseTypes.Tests
{
    [TestClass]
    public class DoubleHistogramTests
    {
        [TestMethod]
        public void EqualRangesHistogramTest()
        {
            // Arrange
            var series = new[]
            {
                -6.237, -6.229, -5.779, -5.139, -4.950, -4.919, -4.636, -4.560, -4.530, -4.526, -4.523, -4.511, -4.409,
                -4.336, -4.259, -4.055, -4.044, -4.006, -3.972, -3.944, -3.829, -3.794, -3.716, -3.542, -3.541, -3.431,
                -3.406, -3.384, -3.307, -3.181, -3.148, -3.124, -3.116, -2.892, -2.785, -2.734, -2.711, -2.637, -2.633,
                -2.428, -2.381, -2.339, -2.276, -2.222, -2.167, -2.111, -2.034, -1.958, -1.854, -1.803, -1.774, -1.755,
                -1.745, -1.713, -1.709, -1.566, -1.548, -1.480, -1.448, -1.353, -1.266, -1.229, -1.179, -1.130, -1.102,
                -1.060, -1.046, -1.035, -0.969, -0.960, -0.903, -0.885, -0.866, -.865, -0.774, -0.721, -0.688, -0.673,
                -0.662, -0.626, -0.543, -0.445, -0.241, -0.174, -0.131, 0.115, 0.205, 0.355, 0.577, 0.591, 0.795, 0.986,
                1.068, 1.099, 1.195, 1.540, 2.008, 2.160, 2.534, 2.848
            };
            var histogram = new DoubleHistogram(series);

            // Act
            var steps = histogram.EqualIntervalHistogram().ToList();

            // Assert
            Assert.AreEqual(steps.Count(), 10);
            Assert.AreEqual(steps.First().Interval.Length, 0.9085, 0.00001);
            CollectionAssert.AreEqual(steps.Select(s => s.ValuesCount).ToList(),
                new double[] {3, 9, 13, 14, 16, 19, 12, 6, 4, 4});
            CollectionAssert.AreEqual(steps.Select(s => Math.Round(s.F, 3)).ToList(),
                new[] {0.033, 0.099, 0.143, 0.154, 0.176, 0.209, 0.132, 0.066, 0.044, 0.044});
        }

        [TestMethod]
        public void EqualProbabilityHistogramTest()
        {
            // Arrange
            var series = new[]
            {
                -6.237, -6.229, -5.779, -5.139, -4.950, -4.919, -4.636, -4.560, -4.530, -4.526, -4.523, -4.511, -4.409,
                -4.336, -4.259, -4.055, -4.044, -4.006, -3.972, -3.944, -3.829, -3.794, -3.716, -3.542, -3.541, -3.431,
                -3.406, -3.384, -3.307, -3.181, -3.148, -3.124, -3.116, -2.892, -2.785, -2.734, -2.711, -2.637, -2.633,
                -2.428, -2.381, -2.339, -2.276, -2.222, -2.167, -2.111, -2.034, -1.958, -1.854, -1.803, -1.774, -1.755,
                -1.745, -1.713, -1.709, -1.566, -1.548, -1.480, -1.448, -1.353, -1.266, -1.229, -1.179, -1.130, -1.102,
                -1.060, -1.046, -1.035, -0.969, -0.960, -0.903, -0.885, -0.866, -0.865, -0.774, -0.721, -0.688, -0.673,
                -0.662, -0.626, -0.543, -0.445, -0.241, -0.174, -0.131, 0.115, 0.205, 0.355, 0.577, 0.591, 0.795, 0.986,
                1.068, 1.099, 1.195, 1.540, 2.008, 2.160, 2.534, 2.848
            };
            var histogram = new DoubleHistogram(series);

            // Act
            var steps = histogram.EqualProbabilityHistogram(10).ToList();

            // Assert
            Assert.AreEqual(steps.Count(), 10);
            Assert.AreEqual(steps.First().Interval.Length, 1.712, 0.001);           
            CollectionAssert.AreEqual(steps.Select(s => Math.Round(s.F, 4)).Take(4).ToList(),
                new[] {0.0584, 0.1567, 0.1385, 0.1316});            
        }
    }
}
