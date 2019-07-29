using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace ArrayBufferNS.Tests
{
    public class CollectionAdderTests
    {
        [Fact]
        public void Test()
        {
            var buffer = new CollectionAdder<string>.BufferContainer(3);

            var list = new List<string>();

            list.AddRange(new CollectionAdder<string>(buffer)
            {
                "aaa",
                "bbb",
                "ccc",
            });

            list.Count.Should().Be(3);
            list[0].Should().Be("aaa");
            list[1].Should().Be("bbb");
            list[2].Should().Be("ccc");
        }

        [Fact]
        public void MultiTest()
        {
            var buffer = new CollectionAdder<string>.BufferContainer(3);

            var list = new List<string>();

            list.AddRange(new CollectionAdder<string>(buffer)
            {
                "aaa",
                "bbb",
                "ccc",
            });
            list.AddRange(new CollectionAdder<string>(buffer)
            {
                "ddd",
                "eee",
                "fff",
            });
            list.AddRange(new CollectionAdder<string>(buffer)
            {
                "ggg",
            });


            list.Count.Should().Be(7);
            list[0].Should().Be("aaa");
            list[1].Should().Be("bbb");
            list[2].Should().Be("ccc");
            list[3].Should().Be("ddd");
            list[4].Should().Be("eee");
            list[5].Should().Be("fff");
            list[6].Should().Be("ggg");
        }
    }
}