using System;
using System.Collections;
using System.Collections.Generic;
using BinaryDictionaryNS;

// ReSharper disable once CheckNamespace
namespace ArrayBufferNS
{
    public class ArrayBuffer<T>
    {
        public struct Span : IDisposable, IEnumerable<T>
        {
            public readonly int Length;

            private readonly int start;
            private readonly ArrayBuffer<T> parent;

            private int last;

            public Span(ArrayBuffer<T> parent, int size)
            {
                start = parent.GetStartPosAndReserve(size);
                Length = size;
                
                this.parent = parent;
                last = 0;
            }

            public T this[int i]
            {
                get => parent.array[start + i];
                set
                {
                    last = -1;
                    parent.array[start + i] = value;
                }
            }

            public void Add(T item)
            {
                if (last == -1)
                {
                    throw new NotSupportedException("Add is only used for collection initialization");
                }

                parent.array[start + last] = item;
                last += 1;
            }

            public void Return()
            {
                for (int i = start; i < start + Length; i++)
                {
                    parent.array[i] = default;
                    parent.usedIndexes.Remove(i);
                }
            }

            public void Dispose()
            {
                Return();
            }

            public IEnumerator<T> GetEnumerator() => new SpanEnumerator(this);

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            private class SpanEnumerator : IEnumerator<T>
            {
                private readonly Span span;
                private int index = -1;
                
                public SpanEnumerator(Span span)
                {
                    this.span = span;
                }


                public bool MoveNext()
                {
                    if (index + 1 >= span.Length)
                    {
                        return false;
                    }
                    
                    index += 1;
                    return true;
                }

                public void Reset()
                {
                    index = -1;
                }

                public T Current => span[index];

                object IEnumerator.Current => Current;

                public void Dispose() => Reset();
            }
        }

        public IReadOnlyList<T> Array => array;

        private T[] array;
        private readonly BinaryDictionary usedIndexes = new BinaryDictionary();

        public ArrayBuffer() : this(10) { }

        public ArrayBuffer(int size)
        {
            array = new T[size];
        }

        public Span Take(int size) => new Span(this, size);

        private int GetStartPosAndReserve(int size)
        {
            CheckArraySize(size);

            int start = -1;
            
            while (true)
            {
                for (int i = start + 1; i < array.Length; i++)
                {
                    if (usedIndexes.Contains(i))
                    {
                        continue;
                    }

                    start = i;
                    break;
                }

                bool isAvailable = true;
                for (int i = start; i < start + size; i++)
                {
                    if (usedIndexes.Contains(i) == false)
                    {
                        continue;
                    }

                    isAvailable = false;
                    break;
                }

                if (isAvailable)
                {
                    break;
                }
            }

            for (int i = start; i < start + size; i++)
            {
                usedIndexes.Add(i);
            }

            return start;
        }
        

        private void CheckArraySize(int size)
        {
            if (size <= array.Length - usedIndexes.Count())
            {
                return;
            }

            var newSize = array.Length * 2;
            if (newSize < size)
            {
                newSize = size * 2;
            }

            var newArray = new T[newSize];
            System.Array.Copy(array, 0, newArray, 0, array.Length);
            array = newArray;
        }
    }
}