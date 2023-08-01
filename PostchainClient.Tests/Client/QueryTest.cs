using Chromia.Encoding;
using Newtonsoft.Json;
using System;
using System.Numerics;
using System.Threading.Tasks;
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
        public async void BufferTest()
        {
            var expected = Signer.PubKey;
            var actual = await Client.Query<Buffer>("test_pubkey", ("pubkey", expected));
            Assert.Equal(expected, actual);
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
    }
}
