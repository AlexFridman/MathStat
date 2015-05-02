using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace BaseTypes
{
    public class DoubleFrequencyFunction
    {
        private readonly int _selectionLength;
        private readonly IEnumerable<double> _statisticalSeries;

        public DoubleFrequencyFunction(IEnumerable<double> statisticalSeries)
        {
            if (statisticalSeries == null)
            {
                throw new ArgumentNullException("statisticalSeries");
            }

            _statisticalSeries = statisticalSeries.OrderBy(i => i);
            _selectionLength = _statisticalSeries.Count();

            BuildStatSeries();
            BuildFunction();
        }

        public IReadOnlyCollection<Item<double>> Frequencies { get; private set; }
        public IReadOnlyCollection<Point<double>> Function { get; private set; }

        private void BuildStatSeries()
        {
            var steps =
                _statisticalSeries.GroupBy(i => i)
                    .Select(
                        group => new Item<double> {Value = @group.Key, Count = @group.Count()})
                    .OrderBy(s => s.Value).ToList();

            Frequencies = new ReadOnlyCollection<Item<double>>(steps);
        }

        private void BuildFunction()
        {
            var points = new List<Point<double>>(Frequencies.Count);

            double currentValue = 0;

            foreach (var item in Frequencies)
            {
                currentValue += item.Count/(double) _selectionLength;
                var point = new Point<double> {X = item.Value, Y = currentValue};

                points.Add(point);
            }

            Function = new ReadOnlyCollection<Point<double>>(points);
        }
    }
}