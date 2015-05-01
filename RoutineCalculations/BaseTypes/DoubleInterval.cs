namespace BaseTypes
{
    public class DoubleInterval : Interval<double>
    {
        public DoubleInterval(double left, double right, bool isLeftClosed = false, bool isRightClosed = false)
            : base(left, right, isLeftClosed, isRightClosed)
        {
        }

        public double Middle
        {
            get { return Left + (Right - Left)/2; }
        }

        public double Length
        {
            get { return (Right - Left); }
        }
    }
}