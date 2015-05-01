using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace BaseTypes
{
    public class DecimalFrequencyFunction
    {
        private readonly IEnumerable<decimal> _statisticalSeries;
        private readonly int _selectionLength;
        public IReadOnlyCollection<Item> Frequencies { get; private set; }
        public IReadOnlyCollection<Point> Function { get; private set; }

        public DecimalFrequencyFunction(IEnumerable<decimal> statisticalSeries)
        {
            if (statisticalSeries == null)
            {
                throw new ArgumentNullException("statisticalSeries");
            }

            _statisticalSeries = statisticalSeries.OrderBy(i => i);
            _selectionLength = _statisticalSeries.Count();

            CalculateFrequencies();
            BuildFunction();
        }

        public struct Item
        {
            public override string ToString()
            {
                return string.Format("Value: {0}, Frequency: {1}", Value, Frequency);
            }

            public decimal Value { get; set; }
            public decimal Frequency { get; set; }
        }

        public struct Point
        {
            public decimal X { get; set; }
            public decimal Y { get; set; }

            public override string ToString()
            {
                return string.Format("X: {0}, Y: {1}", X, Y);
            }
        }

        private void CalculateFrequencies()
        {
            var steps =
                _statisticalSeries.GroupBy(i => i)
                    .Select(
                        group => new Item {Value = @group.Key, Frequency = @group.Count()/(decimal) _selectionLength})
                    .OrderBy(s => s.Value).ToList();

            Frequencies = new ReadOnlyCollection<Item>(steps);
        }

        private void BuildFunction()
        {
            var points = new List<Point>(Frequencies.Count);

            decimal currentValue = 0;

            foreach (var item in Frequencies)
            {
                currentValue += item.Frequency;
                var point = new Point {X = item.Value, Y = currentValue};

                points.Add(point);
            }

            Function = new ReadOnlyCollection<Point>(points);
        }
    }
}