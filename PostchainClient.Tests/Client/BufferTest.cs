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

        [Fact]
        public void EmptyBufferTest()
        {
            var buffer1 = new Buffer();
            Buffer buffer2 = default;
            Assert.True(buffer1.IsEmpty);
            Assert.True(buffer2.IsEmpty);
            Assert.Empty(buffer1.Bytes);
            Assert.Empty(buffer2.Bytes);
        }
    }
}
