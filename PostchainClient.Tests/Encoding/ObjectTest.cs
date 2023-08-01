using Chromia.Encoding;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Numerics;
using Xunit;
using Xunit.Abstractions;

namespace Chromia.Tests.Encoding
{
    public class ObjectTest : PrintableTest
    {
        public ObjectTest(ITestOutputHelper output) : base(output) { }

        [Fact]
        public void NullObjectTest()
        {
            object expected = null;
            var actual = Gtv.Decode(Gtv.Encode(expected));
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void PrimitiveObjectTest()
        {
            var expected = "foo";
            var actual = Gtv.Decode(Gtv.Encode(expected));
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void BufferObjectTest()
        {
            var expected = Buffer.From("affe");
            var actual = Gtv.Decode(Gtv.Encode(expected));
            Assert.Equal(expected.ToString(), actual.ToString());
        }

        [Fact]
        public void BigIntegerObjectTest()
        {
            var expected = BigInteger.One;
            var actual = new BigInteger((long)Gtv.Decode(Gtv.Encode(expected)));
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void SimpleObjectAsJTokenTest()
        {
            var expected = new object[]
            {
                "queryname",
                new Dictionary<string, string>() { { "arg", "foo" } }
            };
            var actual = Gtv.Decode(Gtv.Encode(expected));
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void NestedObjectAsJTokenTest()
        {
            var expected = new object[]
            {
                "queryname",
                new Dictionary<string, object>() {
                    {
                        "arg",
                        new Dictionary<string, string>() { { "arg1", "foo" } }
                    }
                }
            };
            var actual = Gtv.Decode(Gtv.Encode(expected));
            Assert.Equal(expected, actual);
        }



        [Fact]
        public void SimpleObjectTest()
        {
            var expected = new object[]
            {
                "queryname",
                new Dictionary<string, string>() { { "arg", "foo" } }
            };
            var actual = Gtv.Decode(Gtv.Encode(expected));
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void NestedObjectTest()
        {
            var expected = new object[]
            {
                "queryname",
                new Dictionary<string, object>() {
                    {
                        "arg",
                        new Dictionary<string, string>() { { "arg1", "foo" } }
                    }
                }
            };
            var actual = Gtv.Decode(Gtv.Encode(expected));
            Assert.Equal(expected, actual);
        }
    }
}
