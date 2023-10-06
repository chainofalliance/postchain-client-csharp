using Newtonsoft.Json;
using System;
using Xunit;
using Xunit.Abstractions;

namespace Chromia.Tests.Client
{
    public class BufferTest : PrintableTest
    {
        public BufferTest(ITestOutputHelper output) : base(output) { }

        [Fact]
        public void BufferSerializationTest()
        {
            var expected = Buffer.From("affe");
            var serialized = JsonConvert.SerializeObject(expected);
            Console.WriteLine(serialized);
            var actual = JsonConvert.DeserializeObject<Buffer>(serialized);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void BufferImplicitSerializationTest()
        {
            var str = "affe";
            var expected = Buffer.From(str);
            var serialized = JsonConvert.SerializeObject(expected);
            Console.WriteLine(serialized);
            var actual = JsonConvert.DeserializeObject<Buffer>(serialized);
            Assert.Equal(expected, actual);
        }
    }
}
