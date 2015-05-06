using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace BaseTypes.FrequencyFunction
{
    public class DecimalFrequencyFunction
    {
        private readonly int _selectionLength;
        private readonly IEnumerable<decimal> _statisticalSeries;

        public DecimalFrequencyFunction(IEnumerable<decimal> statisticalSeries)
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

        public IReadOnlyCollection<Item<decimal>> Frequencies { get; private set; }
        public IReadOnlyCollection<Point<decimal>> Function { get; private set; }

        private void BuildStatSeries()
        {
            var steps =
                _statisticalSeries.GroupBy(i => i)
                    .Select(
                        group => new Item<decimal> {Value = @group.Key, Count = @group.Count()})
                    .OrderBy(s => s.Value).ToList();

            Frequencies = new ReadOnlyCollection<Item<decimal>>(steps);
        }

        private void BuildFunction()
        {
            var points = new List<Point<decimal>>(Frequencies.Count);

            decimal currentValue = 0;

            foreach (var item in Frequencies)
            {
                currentValue += item.Count/(decimal) _selectionLength;
                var point = new Point<decimal> {X = item.Value, Y = currentValue};

                points.Add(point);
            }

            Function = new ReadOnlyCollection<Point<decimal>>(points);
        }
    }
}