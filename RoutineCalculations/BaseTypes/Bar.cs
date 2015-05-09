using BaseTypes.Interval;

namespace BaseTypes
{
    public class Bar
    {
        private readonly int _overalValuesCount;
        private double _valuesCount;

        public Bar(int overalValuesCount, DoubleInterval interval = null)
        {
            _overalValuesCount = overalValuesCount;
            Interval = interval;
        }

        public DoubleInterval Interval { get; set; }

        public double ValuesCount
        {
            get { return _valuesCount; }
            set
            {
                _valuesCount = value;
                F = _valuesCount / (_overalValuesCount * Interval.Length);
            }
        }

        public double F { get; set; }

        public override string ToString()
        {
            return string.Format("F: {0}, Interval: {1}, ValuesCount: {2}", F, Interval, ValuesCount);
        }
    }
}