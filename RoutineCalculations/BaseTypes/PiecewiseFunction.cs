﻿using System;
using System.Collections.Generic;
using System.Linq;
using BaseTypes.Interval;

namespace BaseTypes
{
    public class PiecewiseFunction<T> where T : IComparable<T>
    {
        public PiecewiseFunction()
        {
            Functions = new Dictionary<Interval<T>, Func<T, T>>();
        }

        public IDictionary<Interval<T>, Func<T, T>> Functions { get; set; }

        public T Calculate(T value)
        {
            try
            {
                var function = Functions.First(f => f.Key.Contains(value));
                return function.Value(value);
            }
            catch (InvalidOperationException)
            {
                throw new InvalidOperationException("Function not exists on this interval");
            }
        }
    }
}