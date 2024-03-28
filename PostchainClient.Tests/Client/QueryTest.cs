using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Xml.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Chromia.Tests.Client
{
    public class QueryTest : PrintableTest, IClassFixture<ChromiaClientFixture>
    {
        private SignatureProvider Signer => StaticSignatureProvider.Signer1;

        private readonly ChromiaClientFixture _fixture;
        private ChromiaClient Client => _fixture.Client;

        public QueryTest(ITestOutputHelper output, ChromiaClientFixture fixture) : base(output)
        {
            _fixture = fixture;
        }

        [Fact]
        public async void NullTest()
        {
            var actual = await Client.Query<object>("test_null");
            Assert.Null(actual);
        }

        [Fact]
        public async void EmptyBufferTest()
        {
            var expected = Buffer.Empty();
            var actual = await Client.Query<Buffer>("test_pubkey", ("pubkey", expected));
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async void BufferTest()
        {
            var expected = Signer.PubKey;
            var actual = await Client.Query<Buffer>("test_pubkey", ("pubkey", expected));
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async void BooleanTest(bool b)
        {
            var actual = await Client.Query<bool>("test_boolean", ("boolean", b));
            Assert.Equal(b, actual);
        }

        [Theory]
        [InlineData("Hello World!")]
        [InlineData("")]
        [InlineData("Swedish: Åå Ää Öö")]
        [InlineData("Danish/Norway: Ææ Øø Åå")]
        [InlineData("German/Finish: Ää Öö Üü")]
        [InlineData("Greek lower: αβγδϵζηθικλμνξοπρστυϕχψω")]
        [InlineData("Greek upper: ΑΒΓΔΕΖΗΘΙΚΛΜΝΞΟΠΡΣΤΥΦΧΨΩ")]
        [InlineData("Russian: АаБбВвГгДдЕеЁёЖжЗзИиЙйКкЛлМмНнОоПпСсТтУуФфХхЦцЧчШшЩщЪъЫыЬьЭэЮюЯя")]
        public async void StringTest(string s)
        {
            var actual = await Client.Query<string>("test_string", ("text", s));
            Assert.Equal(s, actual);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(42424242)]
        [InlineData(-256)]
        [InlineData(long.MinValue)]
        [InlineData(long.MaxValue)]
        public async void TimestampTest(long n)
        {
            var actual = await Client.Query<long>("test_timestamp", ("timestamp", n));
            Assert.Equal(n, actual);
        }

        [Theory]
        [InlineData("0")]
        [InlineData("1")]
        [InlineData("-1")]
        [InlineData("42")]
        [InlineData("-42")]
        [InlineData("-256")]
        [InlineData("256")]
        [InlineData("9223372036854775808")]
        [InlineData("-9223372036854775809")]
        public async void BigIntegerTest(string s)
        {
            var expected = BigInteger.Parse(s);
            var actual = await Client.Query<BigInteger>("test_bigint", ("n", expected));
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData((float)Math.PI)]
        [InlineData(0f)]
        [InlineData(1f)]
        [InlineData(-1f)]
        [InlineData(256f)]
        [InlineData(-256f)]
        [InlineData(float.MaxValue)]
        [InlineData(float.MinValue)]
        [InlineData(1E-20f)]
        public async void FloatTest(float f)
        {
            var expected = f;
            var actual = await Client.Query<float>("test_decimal", ("decimal", expected));
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(Math.PI)]
        [InlineData(0d)]
        [InlineData(1d)]
        [InlineData(-1d)]
        [InlineData(256d)]
        [InlineData(-256d)]
        [InlineData(double.MaxValue)]
        [InlineData(double.MinValue)]
        [InlineData(1E-20d)]
        public async void DoubleTest(double d)
        {
            var expected = d;
            var actual = await Client.Query<double>("test_decimal", ("decimal", expected));
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async void StructTest()
        {
            var expected = new MyStruct()
            {
                A = "foo",
                B = "bar"
            };
            var actual = await Client.Query<MyStruct>("test_struct", ("s", expected));
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async void ReverseStructTest()
        {
            var expected = new MyStructReverse()
            {
                A = "foo",
                B = "bar"
            };
            var actual = await Client.Query<MyStructReverse>("test_struct", ("s", expected));
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async void StructParameterTest()
        {
            var expected = new MyStruct()
            {
                A = "foo",
                B = "bar"
            };
            var actual = await Client.Query<MyStruct>("test_struct", new MyStructQueryObject() { Struct = expected });
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async void NestedStructTest()
        {
            var expected = new MyNestedStruct()
            {
                BigInt = BigInteger.MinusOne,
                Struct = new MyStruct()
                {
                    A = "foo",
                    B = "bar"
                }
            };
            var actual = await Client.Query<MyNestedStruct>("test_nested_struct", ("s", expected));
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async void BigStructTest()
        {
            var expected = new MyBigStruct()
            {
                Bool = true,
                Buffer = Client.BlockchainRID,
                Float = 3.1415f,
                Int = 42,
                Long = -42,
                String = "foo",
                BigInt = BigInteger.MinusOne
            };
            var actual = await Client.Query<MyBigStruct>("test_big_struct", ("s", expected));
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async void EnumTest()
        {
            var expected = MyEnum.V1;
            var actual = await Client.Query<MyEnum>("test_enum", ("e", expected));
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async void Ft3Test()
        {
            var expected = Buffer.From("c42b182e2c6a29d5efa0a38ca337f0226590ebda153da325991fd73d67d3523e");
            var actual = await Client.Query<object>("ft3.get_account_by_id", ("id", expected));
            Console.WriteLine(actual);
        }

        [Fact]
        public async void DictTest()
        {
            var expected = new Dictionary<string, int>() { { "b", 42 }, { "a", 21 } };
            var actual = await Client.Query<Dictionary<string, int>>("test_map2", ("m", expected));
            foreach (var (k, v) in actual)
                Console.WriteLine($"{k}: {v}");
            Assert.Equal(expected, actual);
        }
    }
}
