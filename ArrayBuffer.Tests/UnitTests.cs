using System.Collections.Generic;
using Xunit;
using FluentAssertions;

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
                buffer.Array[i].Should().Be(i);
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

            buffer.Array.Count.Should().Be(40);
            
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
    }
}