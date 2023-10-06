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
    public class OperationTest : PrintableTest
    {
        public OperationTest(ITestOutputHelper output) : base(output) { }

        [Fact]
        public void NoParameterTest()
        {
            var expected = new Operation("test_op");
            var actual = Operation.Decode(expected.Encode());
            Assert.Equal(expected, actual);

            expected = new Operation("test_op2");
            Assert.NotEqual(expected, actual);
        }

        [Fact]
        public void NullParameterTest()
        {
            var expected = new Operation("test_op")
                .AddParameter(null);
            var actual = Operation.Decode(expected.Encode());
            Assert.Equal(expected, actual);

            expected = new Operation("test_op");
            Assert.NotEqual(expected, actual);
        }

        [Fact]
        public void NopTest()
        {
            var expected = Operation.Nop();
            var actual = Operation.Decode(expected.Encode());
            Assert.Equal(expected, actual);

            expected = new Operation("test_op2");
            Assert.NotEqual(expected, actual);
        }

        [Fact]
        public void SingleParameterTest()
        {
            var expected = new Operation("test_op")
                .AddParameter("foo");
            var actual = Operation.Decode(expected.Encode());
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void BigIntegerParameterTest()
        {
            var expected = new Operation("test_op")
                .AddParameter(BigInteger.Zero);
            var actual = Operation.Decode(expected.Encode());
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void BufferParameterTest()
        {
            var expected = new Operation("test_op")
                .AddParameter(Buffer.From("affe"));
            var actual = Operation.Decode(expected.Encode());
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void MultipleParametersTest()
        {
            var expected = new Operation("test_op")
                .AddParameter("foo")
                .AddParameter("bar")
                .AddParameter(Buffer.From("affe"))
                .AddParameter(42);
            var actual = Operation.Decode(expected.Encode());
            Assert.Equal(expected, actual);

            expected = new Operation("test_op")
                .AddParameter(42)
                .AddParameter(Buffer.From("affe"))
                .AddParameter("bar")
                .AddParameter("foo");
            Assert.NotEqual(expected, actual);
        }

        [Fact]
        public void NestedParameterTest()
        {
            var expected = new Operation("test_op")
                .AddParameter(new object[] { "foo", "bar" });
            var actual = Operation.Decode(expected.Encode());
            Assert.Equal(expected, actual);

            expected = new Operation("test_op")
                .AddParameter(new object[] { "bar", "foo" });
            Assert.NotEqual(expected, actual);
        }
    }
}
