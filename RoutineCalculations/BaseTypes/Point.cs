namespace BaseTypes
{
    public struct Point<T>
    {
        public T X { get; set; }
        public T Y { get; set; }

        public override string ToString()
        {
            return string.Format("X: {0}, Y: {1}", X, Y);
        }
    }
}