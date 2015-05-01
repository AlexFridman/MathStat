namespace BaseTypes
{
    public struct Item<T>
    {
        public T Value { get; set; }
        public int Count { get; set; }

        public override string ToString()
        {
            return string.Format("Value: {0}, Frequency: {1}", Value, Count);
        }
    }
}