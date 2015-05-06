using System;
using System.Collections.Generic;

namespace BaseTypes.Interval
{
    public class Interval<T> : IEquatable<Interval<T>> where T : IComparable<T>
    {
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

        public T Left { get; set; }
        public T Right { get; set; }
        public bool IsLeftClosed { get; set; }
        public bool IsRightClosed { get; set; }

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

        #region IEquitable members

        public bool Equals(Interval<T> other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return IsRightClosed == other.IsRightClosed && IsLeftClosed == other.IsLeftClosed &&
                   EqualityComparer<T>.Default.Equals(Right, other.Right) &&
                   EqualityComparer<T>.Default.Equals(Left, other.Left);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Interval<T>) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = IsRightClosed.GetHashCode();
                hashCode = (hashCode*397) ^ IsLeftClosed.GetHashCode();
                hashCode = (hashCode*397) ^ EqualityComparer<T>.Default.GetHashCode(Right);
                hashCode = (hashCode*397) ^ EqualityComparer<T>.Default.GetHashCode(Left);
                return hashCode;
            }
        }

        public static bool operator ==(Interval<T> left, Interval<T> right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Interval<T> left, Interval<T> right)
        {
            return !Equals(left, right);
        }

        #endregion
    }
}