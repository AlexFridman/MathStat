using System;

namespace BaseTypes
{
    public class Interval<T> where T : IComparable<T>
    {
        public T Left { get; set; }
        public T Right { get; set; }
        public bool IsLeftClosed { get; set; }
        public bool IsRightClosed { get; set; }

        public Interval(T left, T right, bool isLeftClosed = false, bool isRightClosed = false)
        {
            if (right.CompareTo(left) < 0)
            {
                throw new ArgumentException("Left can not be greather than rigth.");
            }

            Left = left;
            Right = right;
            IsLeftClosed = isLeftClosed;
            IsRightClosed = isRightClosed;
        }

        public bool Contains(T value)
        {
            var leftBorderComparsion = Left.CompareTo(value);
            var rightBorderComparsion = Right.CompareTo(value);

            if (IsLeftClosed && IsRightClosed)
            {
                return leftBorderComparsion <= 0 && rightBorderComparsion >= 0;
            }
            if (!IsLeftClosed && IsRightClosed)
            {
                return leftBorderComparsion < 0 && rightBorderComparsion >= 0;
            }
            if (IsLeftClosed && !IsRightClosed)
            {
                return leftBorderComparsion <= 0 && rightBorderComparsion > 0;
            }
            return leftBorderComparsion < 0 && rightBorderComparsion > 0;
        }

        public override string ToString()
        {
            return string.Format("{0}{1},{2}{3}",
                IsLeftClosed ? "[" : "(",
                Left,
                Right,
                IsRightClosed ? "]" : ")");
        }
    }
}
