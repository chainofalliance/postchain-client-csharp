using Chromia.Encoding;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Numerics;
using Xunit;
using Xunit.Abstractions;

namespace Chromia.Tests.Encoding
{
    public class GtvTest : PrintableTest
    {
        public GtvTest(ITestOutputHelper output) : base(output) { }

        [Theory]
        [InlineData("")]
        [InlineData("AFFE")]
        [InlineData("E2BE5C617CE50AFD0882A753C6FDA9C4D925EEDAC50DB97E33F457826A856DE0")]
        public void OctetStringTest(string b)
        {
            var expected = new ByteArrayGtv(Buffer.From(b));
            var actual = Gtv.DecodeToGtv(expected.Encode());
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
        public void UTF8StringTest(string s)
        {
            var expected = new StringGtv(s);
            var actual = Gtv.DecodeToGtv(expected.Encode());
            Assert.Equal(expected, actual);
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
        public void BigIntegerTest(string n)
        {
            var i = BigInteger.Parse(n);
            var expected = new BigIntegerGtv(i);
            var actual = Gtv.DecodeToGtv(expected.Encode());
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(42424242)]
        [InlineData(-256)]
        [InlineData(long.MinValue)]
        [InlineData(long.MaxValue)]
        public void IntegerTest(long i)
        {
            var expected = new IntegerGtv(i);
            var actual = Gtv.DecodeToGtv(expected.Encode());
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void EmptyArrayTest()
        {
            var expected = new ArrayGtv(Array.Empty<IGtv>());
            var actual = Gtv.DecodeToGtv(expected.Encode());
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void SimpleArrayTest()
        {
            var expected = new ArrayGtv(new IGtv[]
            {
                new StringGtv("foo"),
                new StringGtv("bar")
            });
            var actual = Gtv.DecodeToGtv(expected.Encode());
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void MixedArrayTest()
        {
            var expected = new ArrayGtv(new IGtv[]
            {
                new StringGtv("foo"),
                new IntegerGtv(1)
            });
            var actual = Gtv.DecodeToGtv(expected.Encode());
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void NestedArrayTest()
        {
            var expected = new ArrayGtv(new IGtv[]
            {
                new ArrayGtv(new IGtv[] { new StringGtv("foo") }),
                new ArrayGtv(new IGtv[] { new StringGtv("bar") })
            });
            var actual = Gtv.DecodeToGtv(expected.Encode());
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("foo", "CBD2B5746BE474CD3C8F2DED0927B9F48B221635F53E1F300C68312DE974F72A")]
        [InlineData("bar", "A741C1E407F18A889E2EFA136C0C9F1600D325363D1E21F0EB1B0DD85FFD9F30")]
        public void HashTest(string val, string expected)
        {
            Assert.Equal(Gtv.Hash(val), Buffer.From(expected));
        }

        [Fact]
        public void DictionaryTest()
        {
            var dict = new Dictionary<uint, Buffer>()
            {
                { 1, ChromiaClient.Hash("foo") }
            };
            var expected = Gtv.Hash(dict);
            var actual = Buffer.From("3A12CCFB1B71FB792E8B78E8A443D532914D1AA1E7DE71D1503F1C6D2C191688");
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(42424242)]
        [InlineData(-256)]
        [InlineData(float.Epsilon)]
        [InlineData(-float.Epsilon)]
        [InlineData(float.MinValue)]
        [InlineData(float.MaxValue)]
        public void FloatTest(float f)
        {
            var expected = Gtv.Hash(f);
            Console.WriteLine(expected);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(42424242)]
        [InlineData(-256)]
        [InlineData(double.Epsilon)]
        [InlineData(-double.Epsilon)]
        [InlineData(long.MinValue)]
        [InlineData(long.MaxValue)]
        public void DoubleTest(double d)
        {
            var expected = Gtv.Hash(d);
            Console.WriteLine(expected);
        }

        [Fact]
        public void SimpleObjectTest()
        {
            var expected = new ArrayGtv(new IGtv[]
            {
                new StringGtv("queryname"),
                new DictGtv(new Dictionary<string, IGtv>()
                {
                    { "arg", new StringGtv("foo") }
                })
            });
            var actual = Gtv.DecodeToGtv(expected.Encode());
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void NestedObjectTest()
        {
            var expected = new ArrayGtv(new IGtv[]
            {
                new StringGtv("queryname"),
                new DictGtv(new Dictionary<string, IGtv>()
                {
                    {
                        "arg",
                        new DictGtv(new Dictionary<string, IGtv>() {
                            { "arg1", new StringGtv("foo") }
                        })
                    }
                })
            });
            var actual = Gtv.DecodeToGtv(expected.Encode());
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("[]")]
        [InlineData("[[]]")]
        [InlineData("[[[]]]")]
        [InlineData("[{}]")]
        [InlineData("[[{}]]")]
        public void EmptyObjectTest(string json)
        {
            var obj = JsonConvert.DeserializeObject(json);
            var hash = ChromiaClient.Hash(obj);
            var actual = Buffer.From("46af9064f12528cad6a7c377204acd0ac38cdc6912903e7dab3703764c8dd5e5");
            Assert.Equal(hash, actual);
        }

        [Fact]
        public void EmptyListTest()
        {
            var obj = Array.Empty<object>();
            var hash = ChromiaClient.Hash(obj);
            var actual = Buffer.From("46af9064f12528cad6a7c377204acd0ac38cdc6912903e7dab3703764c8dd5e5");
            Assert.Equal(hash, actual);
        }

        [Fact]
        public void EmptyNestedListTest()
        {
            var obj = new object[] { Array.Empty<object>(), Array.Empty<object>() };
            var hash = ChromiaClient.Hash(obj);
            var actual = Buffer.From("8f0402234fe66da0b21f7c871cc2f3211fe8cee5af714bdcdb76bd9ef848b1fd");
            Assert.Equal(hash, actual);
        }

        [Fact]
        public void ListEqualToArrayTest()
        {
            var list = new List<int>() { int.MinValue, -1, 0, 1, int.MaxValue };
            var hash = ChromiaClient.Hash(list);
            var actual = ChromiaClient.Hash(list.ToArray());
            Assert.Equal(hash, actual);
        }

        [Fact]
        public void ObjectTest()
        {
            var obj = JsonConvert.DeserializeObject("[\"Guardian\",0,[],[1,2,3],[]]");
            var gtv = ChromiaClient.EncodeToGtv(obj);
            var actualGtv = Buffer.From("a52e302ca20a0c08477561726469616ea303020100a5023000a511300fa303020101a303020102a303020103a5023000");
            Assert.Equal(gtv, actualGtv);

            var hash = ChromiaClient.Hash(obj);
            var actualHash = Buffer.From("b532f964a6969bcd8f7f509fe13cc5d3dc70e702d8f939edba90af8150f9e30d");
            Assert.Equal(hash, actualHash);
        }

        [Theory]
        [InlineData(
            "a50e300ca20a0c08477561726469616e",
            "ceeadb93aa2c94f2f141d1704d1f21eed10cdfae8dd46682ab1026d7126d9640"
        )]
        [InlineData(
            "a5133011a20a0c08477561726469616ea303020100",
            "2a929e1cbb3bed434194689aac585d06fa14cb33154030d322a087ea78af7e76"
        )]
        [InlineData(
            "a5173015a20a0c08477561726469616ea303020100a5023000",
            "adfe457e02d1a0196214020a86ea1385ce1dc389570793581806d0ace82a6006"
        )]
        [InlineData(
            "a51b3019a20a0c08477561726469616ea303020100a5023000a5023000",
            "1d2e9a3eb055266e1573cc8b70c0ec3112b848b773b4d79958b04f1983261048"
        )]
        [InlineData(
            "a52e302ca20a0c08477561726469616ea303020100a5023000a5023000a511300fa303020101a303020102a303020103",
            "02401767111b7f2b5b8b6e64a8a6d51392926276724868bb26fbc915d4fd4f63"
        )]
        public void GuardianTest(string gtv, string expected)
        {
            var obj = ChromiaClient.DecodeFromGtv(Buffer.From(gtv));

            var hash = ChromiaClient.Hash(obj);
            var expectedHash = Buffer.From(expected);
            Assert.Equal(expectedHash, hash);
        }
    }
}
