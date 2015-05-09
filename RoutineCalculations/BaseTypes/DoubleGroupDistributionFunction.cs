using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace BaseTypes
{
    public class DoubleGroupDistributionFunction
    {
        private readonly IEnumerable<Bar> _groupedStatisticalSeries;
        private readonly int _totalValuesCount;

        public DoubleGroupDistributionFunction(IEnumerable<BaseTypes.Bar> groupedStatisticalSeries, int totalValuesCount)
        {            
            if (groupedStatisticalSeries == null)
            {
                throw new ArgumentNullException("groupedStatisticalSeries");
            }
            if (totalValuesCount < 1)
            {
                throw new ArgumentOutOfRangeException("totalValuesCount");
            }

            _totalValuesCount = totalValuesCount;
            BuildFunction();
        }

        public IReadOnlyCollection<Point<double>> Function { get; private set; }


        private void BuildFunction()
        {
            var points = new List<Point<double>>();

            double currentValue = 0;

            foreach (var group in _groupedStatisticalSeries)
            {
                currentValue += group.ValuesCount/(double)_totalValuesCount;
                var point = new Point<double> {X = group.Interval.Left, Y = currentValue};

                points.Add(point);
            }

            Function = new ReadOnlyCollection<Point<double>>(points);
        }
    }
}