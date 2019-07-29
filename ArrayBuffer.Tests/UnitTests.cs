using System;
using System.Collections.Generic;
using Xunit;
using FluentAssertions;
using static FluentAssertions.FluentActions;

// ReSharper disable once CheckNamespace
namespace ArrayBufferNS.Tests
{
    public class UnitTests
    {
        [Fact]
        public void TestSmallArray()
        {
            var buffer = new ArrayBuffer<int>(10);

            var span = buffer.Take(10);
            for (int i = 0; i < span.Length; i++)
            {
                span[i] = i;
            }

            for (int i = 0; i < span.Length; i++)
            {
                span[i].Should().Be(i);
            }

            for (int i = 0; i < span.Length; i++)
            {
                buffer.Array[i].Should().Be(i);
            }
        }

        [Fact]
        public void TestArrayOverInitialSize()
        {
            var buffer = new ArrayBuffer<int>(10);

            var span = buffer.Take(20);
            for (int i = 0; i < span.Length; i++)
            {
                span[i] = i;
            }

            for (int i = 0; i < span.Length; i++)
            {
                span[i].Should().Be(i);
            }

            for (int i = 0; i < span.Length; i++)
            {
                buffer.Array[i + 10].Should().Be(i);
            }
        }

        [Fact]
        public void TestMoreThan1Take()
        {
            var buffer = new ArrayBuffer<int>(10);

            var span1 = buffer.Take(10);
            var span2 = buffer.Take(10);

            for (int i = 0; i < span2.Length; i++)
            {
                span2[i] = i;
            }

            for (int i = 0; i < span1.Length; i++)
            {
                span1[i].Should().Be(0);
            }

            for (int i = 0; i < span2.Length; i++)
            {
                span2[i].Should().Be(i);
            }
        }

        [Fact]
        public void TestReuse()
        {
            var buffer = new ArrayBuffer<int>(10);
            
            var span = buffer.Take(10);
            for (int i = 0; i < span.Length; i++)
            {
                span[i] = (i + 1) * 10;
            }

            span.Return();
            span = buffer.Take(10);
            for (int i = 0; i < span.Length; i++)
            {
                span[i].Should().Be(0);
            }

            buffer.Array.Count.Should().Be(10);
        }

        [Fact]
        public void TestReuseAndCheckMiddle()
        {
            var buffer = new ArrayBuffer<int>(10);

            var span1 = buffer.Take(10);
            var span2 = buffer.Take(10);
            var span3 = buffer.Take(10);

            buffer.Array.Count.Should().Be(30);

            for (int i = 0; i < span2.Length; i++)
            {
                span2[i] = i;
            }
            
            span2.Return();

            for (int i = 0; i < 10; i++)
            {
                span1[i].Should().Be(0);
                span2[i].Should().Be(0);
                span3[i].Should().Be(0);
            }

            for (int i = 10; i < 20; i++)
            {
                buffer.Array[i].Should().Be(0);
            }
        }

        [Fact]
        public void TestCollectionInitializer()
        {
            var buffer = new ArrayBuffer<int>(10);
            var span = new ArrayBuffer<int>.Span(buffer, 5)
            {
                0,
                1,
                2,
                3,
                4,
                5
            };

            for (int i = 0; i < span.Length; i++)
            {
                span[i].Should().Be(i);
            }
        }

        [Fact]
        public void TestSpanEnumerator()
        {
            var buffer = new ArrayBuffer<int>(10);
            var span = buffer.Take(10);
            for (int i = 0; i < 10; i++)
            {
                span[i] = i;
            }

            var cnt = 0;
            foreach (var i in span)
            {
                i.Should().Be(cnt);
                cnt += 1;
            }
        }

        [Fact]
        public void TestListAddRange()
        {
            var buffer = new ArrayBuffer<string>(3);
            var list = new List<string>();

            using (var span = new ArrayBuffer<string>.Span(buffer, 3) {"aaa", "bbb", "ccc"})
            {
                list.AddRange(span);
            }

            using (var span = new ArrayBuffer<string>.Span(buffer, 3) {"ddd", "eee", "fff"})
            {
                list.AddRange(span);
            }

            using (var span = new ArrayBuffer<string>.Span(buffer, 3) {"ggg", "hhh", "iii"})
            {
                list.AddRange(span);
            }

            buffer.Array.Count.Should().Be(3);
            list[0].Should().Be("aaa");
            list[1].Should().Be("bbb");
            list[2].Should().Be("ccc");
            list[3].Should().Be("ddd");
            list[4].Should().Be("eee");
            list[5].Should().Be("fff");
            list[6].Should().Be("ggg");
            list[7].Should().Be("hhh");
            list[8].Should().Be("iii");
        }

        [Fact]
        public void TestThatOnceReturnedCantUse()
        {
            var buffer = new ArrayBuffer<int>(10);

            var span = buffer.Take(10);
            for (int i = 0; i < span.Length; i++)
            {
                span[i] = 1;
            }

            span.Return();
            
            span.Returned.Should().Be(true);

            span[0].Should().Be(0);

            span[0] = 10;
            span[0].Should().Be(0);
        }

        [Fact]
        public void TestArrayExtension()
        {
            var array = new[] {0, 1, 2, 3};

            array = ArrayBuffer<int>.ExtendArray(array, 3);
            array.Length.Should().Be(7);
            array[0].Should().Be(0);
            array[1].Should().Be(1);
            array[2].Should().Be(2);
            array[3].Should().Be(3);
            for (int i = 4; i < array.Length; i++)
            {
                array[i].Should().Be(0);
            }
        }

        [Fact]
        public void TestMapExtension()
        {
            var map = new[]
            {
                new Map(10, 0),
                new Map(10, 0),
            };

            map = ArrayBuffer<int>.ExtendMap(map, 10);

            map.Length.Should().Be(2);
            map[0].Position.Should().Be(10);
            map[0].Size.Should().Be(10);
            map[1].Position.Should().Be(20);
            map[1].Size.Should().Be(0);
        }
        [Fact]
        public void TestMapExtensionWithLargerNumber()
        {
            var map = new[]
            {
                new Map(0, 10),
                new Map(10, 0),
            };

            map = ArrayBuffer<int>.ExtendMap(map, 20);

            map.Length.Should().Be(3);
            map[0].Position.Should().Be(0);
            map[0].Size.Should().Be(10);
            map[1].Position.Should().Be(10);
            map[1].Size.Should().Be(20);
            map[2].Position.Should().Be(30);
            map[2].Size.Should().Be(0);
        }

        [Fact]
        public void TestSquashMap()
        {
            var map = new[]
            {
                new Map(0, 0),
                new Map(10, 0),
            };

            map = ArrayBuffer<int>.SquashMap(map, 0);
            map.Length.Should().Be(2);
            map[0].Position.Should().Be(10);
            map[0].Size.Should().Be(0);
            map[1].Position.Should().Be(10);
            map[1].Size.Should().Be(0);
        }

        [Fact]
        public void TestSquashMapLarge()
        {
            var map = new[]
            {
                new Map(0, 0),
                new Map(10, 10),
                new Map(20, 10),
                new Map(30, 10),
                new Map(40, 10),
                new Map(50, 0),
            };

            map = ArrayBuffer<int>.SquashMap(map, 0);
            for (int i = 0; i < 4; i++)
            {
                map[i].Position.Should().Be((i + 1) * 10);
                map[i].Size.Should().Be(10);
            }

            map[4].Position.Should().Be(50);
            map[4].Size.Should().Be(0);
            map[5].Position.Should().Be(50);
            map[5].Size.Should().Be(0);
        }

        [Fact]
        public void TestFindOpenLocation()
        {
            var maps = new[]
            {
                new Map(0, 10),
                new Map(10, 0),
            };

            var (map, i) = ArrayBuffer<int>.FindOpenLocation(maps, 10);
            i.Should().Be(0);
            map.Position.Should().Be(0);
            map.Size.Should().Be(10);

            maps = new[]
            {
                new Map(0, 10),
                new Map(10, 10),
                new Map(20, 20),
                new Map(40, 10),
                new Map(50, 0),
            };
            
            (map, i) = ArrayBuffer<int>.FindOpenLocation(maps, 20);
            i.Should().Be(2);
            map.Position.Should().Be(20);
            map.Size.Should().Be(20);

            maps = new[]
            {
                new Map(10, 10),
            };

            Invoking(() => ArrayBuffer<int>.FindOpenLocation(maps, 20)).Should().Throw<Exception>();
        }

        [Fact]
        public void TestAllocate()
        {
            var buffer = new ArrayBuffer<int>();
            buffer.Allocate(10);
            buffer.Map[0].Position.Should().Be(10);
            buffer.Map[0].Size.Should().Be(0);

            buffer.Allocate(10);
            buffer.Map[0].Position.Should().Be(20);
            buffer.Map[0].Size.Should().Be(0);

            buffer.Array.Count.Should().Be(20);
        }

        [Fact]
        public void TestFindFreeLocation()
        {
            var maps = new[]
            {
                new Map(0, 10),
                new Map(10, 10),
                new Map(20, 0),
            };
            var freeLoc = ArrayBuffer<int>.FindFreeLocation(maps, 0);
            freeLoc.Should().Be(1);
        }

        [Fact]
        public void TestFree()
        {
            var buffer = new ArrayBuffer<int>();
            int a = buffer.Allocate(10);
            int b = buffer.Allocate(20);
            int c = buffer.Allocate(10);
            buffer.Free(20, b);
            buffer.Free(10, a);
            buffer.Free(10, c);
        }
    }
}