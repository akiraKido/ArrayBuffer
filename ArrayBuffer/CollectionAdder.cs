using System;
using System.Collections;
using System.Collections.Generic;

namespace ArrayBufferNS
{
    public class CollectionAdder<T> : IEnumerable<T>
    {
        public struct BufferContainer
        {
            public T[] Buffer;

            public BufferContainer(int size) => Buffer = new T[size];

            internal void DoubleSize() => Array.Resize(ref Buffer, Buffer.Length * 2);
        }

        private BufferContainer container;
        private int offset = 0;

        public CollectionAdder(BufferContainer container)
        {
            this.container = container;
        }

        public void Add(T item)
        {
            if (offset >= container.Buffer.Length)
            {
                container.DoubleSize();
            }

            container.Buffer[offset++] = item;
        }

        public IEnumerator<T> GetEnumerator() => new CollectionAdderEnumerator(container, this);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private class CollectionAdderEnumerator : IEnumerator<T>
        {
            private int offset = -1;
            private readonly BufferContainer container;
            private readonly CollectionAdder<T> adder;

            internal CollectionAdderEnumerator(BufferContainer container, CollectionAdder<T> adder)
            {
                this.container = container;
                this.adder = adder;
            }

            public bool MoveNext() => ++offset < adder.offset;

            public void Reset() => offset = -1;

            public T Current => container.Buffer[offset];

            object IEnumerator.Current => Current;

            public void Dispose() { }
        }
    }
}