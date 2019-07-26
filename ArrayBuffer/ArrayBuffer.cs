using System;
using System.Collections.Generic;
using BinaryDictionaryNS;

// ReSharper disable once CheckNamespace
namespace ArrayBufferNS
{
    public class ArrayBuffer<T>
    {
        public struct Span : IDisposable
        {
            public readonly int Length;

            private readonly int start;
            private readonly ArrayBuffer<T> parent;

            public Span(ArrayBuffer<T> parent, int start, int length)
            {
                this.start = start;
                Length = length;

                this.parent = parent;
            }

            public T this[int i]
            {
                get => parent.array[start + i];
                set => parent.array[start + i] = value;
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
        }

        public IReadOnlyList<T> Array => array;

        private T[] array;
        private readonly BinaryDictionary usedIndexes = new BinaryDictionary();

        public ArrayBuffer() : this(10) { }

        public ArrayBuffer(int size)
        {
            array = new T[size];
        }

        public Span Take(int size)
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

            return new Span(this, start, size);
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