using System;
using System.Collections;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace ArrayBufferNS
{
    // Unix v7
    // https://sites.google.com/site/lionscommentaryonunixreading/v7listing/mallocc
    public class Map
    {
        public int Position { get; internal set; }
        public int Size { get; internal set; }

        public Map(int position, int size)
        {
            Position = position;
            Size = size;
        }
    }

    public class ArrayBuffer<T>
    {
        public struct Span : IDisposable, IEnumerable<T>
        {
            public readonly int Length;

            private readonly int start;
            private readonly ArrayBuffer<T> parent;

            private int last;
            public bool Returned { get; private set; }

            public Span(ArrayBuffer<T> parent, int size)
            {
                start = parent.Allocate(size);
                Length = size;
                
                this.parent = parent;
                last = 0;
                Returned = false;

                for (int i = start; i < start + size; i++)
                {
                    parent.array[i] = default;
                }
            }

            public T this[int i]
            {
                get => Returned ? default : parent.array[start + i];
                set
                {
                    if (Returned)
                    {
                        return;
                    }

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
                Returned = true;

                parent.Free(Length, start);
            }

            public void Dispose()
            {
                Return();
            }

            public IEnumerator<T> GetEnumerator() => new SpanEnumerator(this);

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            private struct SpanEnumerator : IEnumerator<T>
            {
                private readonly Span span;

                private int index;

                public SpanEnumerator(Span span)
                {
                    this.span = span;
                    index = -1;
                }

                public bool MoveNext()
                {
                    if (span.Returned)
                    {
                        return false;
                    }
                    
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

        internal IReadOnlyList<T> Array => array;

        private T[] array;

        public ArrayBuffer() : this(10) { }

        public ArrayBuffer(int size)
        {
            array = new T[size];

            map = new Map[2];
            map[0] = new Map(0, size);
            map[1] = new Map(size, 0);
        }

        public Span Take(int size) => new Span(this, size);

        internal IReadOnlyList<Map> Map => map;
        private Map[] map;

        internal int Allocate(int size)
        {
            while (true)
            {
                var (openMap, i) = FindOpenLocation(map, size);

                if (openMap.Size == 0)
                {
                    // Extend Array
                    array = ExtendArray(array, size);
                    map = ExtendMap(map, size);
                    continue;
                }

                var start = openMap.Position;
                openMap.Position += size;

                if ((openMap.Size -= size) == 0)
                {
                    map = SquashMap(map, i);
                }

                return start;
            }
        }

        internal static (Map map, int i) FindOpenLocation(Map[] maps, int size)
        {
            for (int i = 0; i < maps.Length; i++)
            {
                var map = maps[i];
                if (map.Size == 0)
                {
                    return (map, i);
                }

                if (map.Size < size)
                {
                    continue;
                }

                return (map, i);
            }

            throw new Exception("Tailing size 0 Map seems to have disappeared?");
        }

        internal static T[] ExtendArray(T[] array, int size)
        {
            var newArray = new T[array.Length + size];
            System.Array.Copy(array, 0, newArray, 0, array.Length);
            return newArray;
        }

        internal static Map[] ExtendMap(Map[] maps, int size)
        {
            int tail = -1;
            if (maps[0].Size == 0)
            {
                tail = 0;
            }
            else
            {
                for (int i = 0; i < maps.Length; i++)
                {
                    if (maps[i].Size > 0)
                    {
                        continue;
                    }

                    tail = i;
                    break;
                }

                if (tail == maps.Length - 1)
                {
                    var newArray = new Map[maps.Length + 1];
                    System.Array.Copy(maps, 0, newArray, 0, maps.Length);
                    maps = newArray;
                }
            }

            var newMap = new Map(maps[tail].Position, size);
            maps[tail].Position += size;

            maps[tail + 1] = maps[tail];
            maps[tail] = newMap;

            return maps;
        }

        internal static Map[] SquashMap(Map[] map, int startLoc)
        {
            do
            {
                startLoc += 1;

                map[startLoc - 1].Position = map[startLoc].Position;
                map[startLoc - 1].Size = map[startLoc].Size;
            } while (map[startLoc - 1].Size > 0);

            return map;
        }

        internal void Free(int size, int pos)
        {
            int loc = FindFreeLocation(map, pos);

            var bp = map[loc];

            if (loc > 0 && map[loc - 1].Position + map[loc - 1].Size == pos)
            {
                var prevBp = map[loc - 1];

                prevBp.Size += size;
                if (pos + size != bp.Position)
                {
                    return;
                }

                prevBp.Size += bp.Size;
                while (bp.Size > 0)
                {
                    loc += 1;
                    prevBp.Position = bp.Position;
                    prevBp.Size = bp.Size;
                }
            }
            else
            {
                if (pos + size == bp.Position && bp.Size > 0)
                {
                    bp.Position -= size;
                    bp.Size += size;
                }
                else
                {
                    while (size > 0)
                    {
                        var t = bp.Position;
                        bp.Position = pos;
                        pos = t;

                        t = bp.Size;
                        bp.Size = size;
                        size = t;

                        loc += 1;
                        bp = map[loc];
                    }
                }
            }
        }

        internal static int FindFreeLocation(Map[] map, int pos)
        {
            int loc = -1;
            for (int i = 0; i < map.Length; i++)
            {
                loc = i;
                var bp = map[i];
                if (bp.Position <= pos && bp.Size != 0)
                {
                    continue;
                }

                break;
            }

            return loc;
        }
    }
}