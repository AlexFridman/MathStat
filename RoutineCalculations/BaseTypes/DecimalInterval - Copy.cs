namespace BaseTypes
{
    public class DecimalInterval : Interval<decimal>
    {
        public decimal Middle
        {
            get { return Left + (Right - Left)/2; }
        }

        public decimal Length
        {
            get { return (Right - Left); }
        }

        public DecimalInterval(decimal left, decimal right, bool isLeftClosed = false, bool isRightClosed = false)
            : base(left, right, isLeftClosed, isRightClosed)
        {
        }
    }
}