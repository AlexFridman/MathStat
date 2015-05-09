using System;
using System.Collections;
using System.Collections.Generic;

namespace BaseTypes
{
    public class SequenceGenerator<T> : IEnumerable<T>, IEnumerator<T> where T : IComparable<T>
    {
        private readonly Func<T, T> _function;
        private readonly Func<T, T> _nextMemberGenerator;
        private readonly T _startValue;
        private readonly int _stepCount;
        private T _currentMember;
        private int _step;

        public SequenceGenerator(T start, int count, Func<T, T> nextMemberGenerator, Func<T, T> function)
        {
            if (count < 0)
            {
                throw new ArgumentNullException("count", "Count can not be less than zero.");
            }
            if (nextMemberGenerator == null)
            {
                throw new ArgumentNullException("nextMemberGenerator");
            }
            if (function == null)
            {
                throw new ArgumentNullException("function");
            }

            _currentMember = default(T);
            _step = -1;
            _startValue = start;
            _stepCount = count;
            _nextMemberGenerator = nextMemberGenerator;
            _function = function;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Dispose()
        {            
        }

        public bool MoveNext()
        {
            _currentMember = _step == -1 ? _startValue : _nextMemberGenerator(_currentMember);

            Current = _function(_currentMember);

            _step++;
            return _step < _stepCount;
        }

        public void Reset()
        {
            _step = -1;
            _currentMember = default(T);
            Current = default(T);
        }

        public T Current { get; private set; }

        object IEnumerator.Current
        {
            get { return Current; }
        }
    }
}