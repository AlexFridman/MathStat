using System;
using System.Collections.Generic;
using System.Linq;
using BaseTypes.Interval;

namespace BaseTypes
{
    public class DoubleHistogram
    {
        

        private readonly IList<double> _orederdVariationalSeries;
        private readonly double _minValue;
        private readonly double _maxValue;

        private readonly int _varitionalSeriesLength;

        public DoubleHistogram(IEnumerable<double> variationalSeries)
        {
            _orederdVariationalSeries = variationalSeries.OrderBy(v => v).ToList();
            _minValue = _orederdVariationalSeries.Min();
            _maxValue = _orederdVariationalSeries.Max();
            _varitionalSeriesLength = _orederdVariationalSeries.Count;
        }

        public IEnumerable<Bar> EqualIntervalHistogram()
        {
            var intervalCount = DefineBarCount(_varitionalSeriesLength); // M

            var intervals = BuildEqualIntervals(_minValue, _maxValue, intervalCount);
            var histogramSteps =
                intervals.Select(i => new Bar(_varitionalSeriesLength) {Interval = i, ValuesCount = 0}).ToList();

            for (var i = 0; i < _varitionalSeriesLength; i++)
            {
                var value = _orederdVariationalSeries[i];

                var ownerSteps = histogramSteps.Where(step => step.Interval.Contains(value)).ToList();

                if (!ownerSteps.Any())
                {
                    throw new Exception("Intervals buit incorrectly.");
                }
                if (ownerSteps.Count() == 2)
                {
                    foreach (var step in ownerSteps)
                    {
                        step.ValuesCount += 0.5;
                    }
                }
                else
                {
                    ownerSteps.First().ValuesCount += 1;
                }
            }

            return histogramSteps;
        }

        private int DefineBarCount(int seriesLength)
        {
            int intervalCount; // M

            if (_varitionalSeriesLength <= 10000)
            {
                intervalCount = (int) Math.Sqrt(_varitionalSeriesLength);
            }
            else
            {
                intervalCount = (int) (0.5*Math.Log10(_varitionalSeriesLength));
            }

            return intervalCount;
        }

        private IEnumerable<DoubleInterval> BuildEqualIntervals(double minValue, double maxValue, int intervalCount)
        {
            var intervalLength = (maxValue - minValue)/intervalCount;
            var intervals = new List<DoubleInterval>(intervalCount);

            var tempLeft = _minValue;
            var tempRight = tempLeft + intervalLength;

            for (var i = 0; i < intervalCount; i++)
            {
                var interval = new DoubleInterval(tempLeft, tempRight, true, true);
                intervals.Add(interval);

                tempLeft = tempRight;
                tempRight += intervalLength;
            }

            return intervals;
        }

        public IEnumerable<Bar> EqualProbabilityHistogram(int intervalCount)
        {
            if (intervalCount < 1)
            {
                throw new ArgumentOutOfRangeException("intervalCount");
            }
            if (!IsCorrectIntervalCountForEqualProbabilityHistogram(_varitionalSeriesLength, intervalCount))
            {
                throw new ArgumentException("Incorrect interval count.", "intervalCount");
            }

            var valuesPerInterval = _varitionalSeriesLength/intervalCount; // h
            var bars = new List<Bar>(intervalCount);
            double tempLeft = _orederdVariationalSeries.First();
            double tempRight = (_orederdVariationalSeries.Skip(valuesPerInterval).First() +
                                _orederdVariationalSeries.Take(valuesPerInterval).Last()
                )/2;

            for (var i = 0; i < intervalCount; i++)
            {
                var bar = new Bar(_varitionalSeriesLength)
                {
                    Interval = new DoubleInterval(tempLeft, tempRight),
                    ValuesCount = valuesPerInterval
                };

                bars.Add(bar);

                tempLeft = tempRight;

                if (i < intervalCount - 2)
                {
                    tempRight = (_orederdVariationalSeries.Skip((i + 2) * valuesPerInterval).First() +
                                    _orederdVariationalSeries.Take((i + 2) * valuesPerInterval).Last()) / 2;    
                }
                else
                {
                    tempRight = _orederdVariationalSeries.Last();
                }
                
            }

            return bars;
        }

        private bool IsCorrectIntervalCountForEqualProbabilityHistogram(int seriesLength, int intervalCount)
        {
            return seriesLength%intervalCount == 0;
        }

        public IEnumerable<Point<double>> BuildPolygon(IEnumerable<Bar> bars)
        {
            var sortedBars = bars.OrderBy(b => b.Interval.Middle).ToList();

            return sortedBars.Select(b => new Point<double>(b.Interval.Middle, b.F));
        }
    }
}