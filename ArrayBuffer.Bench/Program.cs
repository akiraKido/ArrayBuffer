using System.Collections.Generic;
using ArrayBufferNS;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace ArrayBuffer.Bench
{
    internal static class Program
    {
        private static void Main()
        {
            BenchmarkRunner.Run<Tests>();
        }
    }

    [MemoryDiagnoser]
    public class Tests
    {
        [Benchmark]
        public List<string> NormalListAndAdd()
        {
            var list = new List<string>();
            list.Add("aaa");
            list.Add("bbb");
            list.Add("ccc");
            list.Add("ddd");
            list.Add("eee");
            list.Add("fff");
            list.Add("ggg");
            return list;
        }

        [Benchmark]
        public List<string> ListAndArray()
        {
            var list = new List<string>();
            list.AddRange(new[]
            {
                "aaa",
                "bbb",
                "ccc"
            });
            list.AddRange(new[]
            {
                "ddd",
                "eee",
                "fff"
            });
            list.AddRange(new[]
            {
                "ggg"
            });
            return list;
        }

        [Benchmark]
        public List<string> ListAndArrayBuffer()
        {
            var buffer = new ArrayBuffer<string>(3);
            var list = new List<string>();

            var span = new ArrayBuffer<string>.Span(buffer, 3)
            {
                "aaa",
                "bbb",
                "ccc"
            };
            list.AddRange(span);
            span.Return();
            
            span = new ArrayBuffer<string>.Span(buffer, 3)
            {
                "ddd",
                "eee",
                "fff"
            };
            list.AddRange(span);
            span.Return();

            span = new ArrayBuffer<string>.Span(buffer, 1)
            {
                "ggg"
            };
            list.AddRange(span);
            span.Return();
            
            return list;
        }
        
        [Benchmark]
        public List<string> ListAndArrayBufferWithLocalFunction()
        {
            var buffer = new ArrayBuffer<string>(3);
            var list = new List<string>();

            void AddRange(ArrayBuffer<string>.Span span)
            {
                using (span) list.AddRange(span);
            }

            AddRange(new ArrayBuffer<string>.Span(buffer, 3)
            {
                "aaa",
                "bbb",
                "ccc"
            });
            AddRange(new ArrayBuffer<string>.Span(buffer, 3)
            {
                "ddd",
                "eee",
                "fff"
            });
            AddRange(new ArrayBuffer<string>.Span(buffer, 1)
            {
                "ggg"
            });
            return list;
        }
    }
}