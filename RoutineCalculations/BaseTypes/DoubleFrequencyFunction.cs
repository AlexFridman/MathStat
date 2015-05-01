using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace BaseTypes
{
    public class DoubleFrequencyFunction
    {
        private readonly IEnumerable<double> _statisticalSeries;
        private readonly int _selectionLength;
        public IReadOnlyCollection<Item> Frequencies { get; private set; }
        public IReadOnlyCollection<Point> Function { get; private set; }

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

        public struct Item
        {
            public override string ToString()
            {
                return string.Format("Value: {0}, Frequency: {1}", Value, Count);
            }

            public double Value { get; set; }
            public int Count { get; set; }
        }

        public struct Point
        {
            public double X { get; set; }
            public double Y { get; set; }

            public override string ToString()
            {
                return string.Format("X: {0}, Y: {1}", X, Y);
            }
        }

        private void BuildStatSeries()
        {
            var steps =
                _statisticalSeries.GroupBy(i => i)
                    .Select(
                        group => new Item {Value = @group.Key, Count = @group.Count()})
                    .OrderBy(s => s.Value).ToList();

            Frequencies = new ReadOnlyCollection<Item>(steps);
        }

        private void BuildFunction()
        {
            var points = new List<Point>(Frequencies.Count);

            double currentValue = 0;

            foreach (var item in Frequencies)
            {
                currentValue += item.Count/(double) _selectionLength;
                var point = new Point {X = item.Value, Y = currentValue};

                points.Add(point);
            }

            Function = new ReadOnlyCollection<Point>(points);
        }
    }
}