namespace BaseTypes
{
    public struct Point<T>
    {
        public T X { get; set; }
        public T Y { get; set; }

        public Point(T x, T y) : this()
        {
            X = x;
            Y = y;
        }
        public override string ToString()
        {
            return string.Format("X: {0}, Y: {1}", X, Y);
        }
    }
}