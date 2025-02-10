using Chromia.Encoding;
using Chromia.Tests.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

namespace Chromia.Tests.Encoding
{
    public class HashTest : PrintableTest
    {
        public HashTest(ITestOutputHelper output) : base(output) { }


        [Theory]
        [InlineData("foo", "CBD2B5746BE474CD3C8F2DED0927B9F48B221635F53E1F300C68312DE974F72A")]
        [InlineData("bar", "A741C1E407F18A889E2EFA136C0C9F1600D325363D1E21F0EB1B0DD85FFD9F30")]
        public void StringHashTest(string val, string expected)
        {
            Assert.Equal(Gtv.Hash(val), Buffer.From(expected));
        }

        [Fact]
        public void IntStringTest()
        {
            var actual = Gtv.Hash("1");
            var expected = Buffer.From("4C3896768D311786D9B8507ED4E0A49F4DD5A1B5B630F73A87F43FD907D3EFAB");
            Assert.Equal(expected.Parse(), actual.Parse());
        }

        [Fact]
        public void DictionaryTest()
        {
            var dict = new Dictionary<string, Buffer>()
            {
                { "1", ChromiaClient.Hash("foo") }
            };
            var actual = Gtv.Hash(dict);
            var expected = Buffer.From("7AE617AAA57255D40E5C8D9F284C872EBEC5CBCD2B0C551992D4389CC77E5181");
            Assert.Equal(expected.Parse(), actual.Parse());
        }

        [Fact]
        public void DictionaryIntKeyTest()
        {
            var dict = new Dictionary<uint, Buffer>()
            {
                { 1, ChromiaClient.Hash("foo") }
            };
            var actual = Gtv.Hash(dict);
            var expected = Buffer.From("7AE617AAA57255D40E5C8D9F284C872EBEC5CBCD2B0C551992D4389CC77E5181");
            Assert.Equal(expected.Parse(), actual.Parse());
        }

        [Fact]
        public void DictionaryBufferKeyTest()
        {
            var dict = new Dictionary<Buffer, Buffer>()
            {
                { ChromiaClient.Hash("foo"), ChromiaClient.Hash("bar") }
            };
            var actual = Gtv.Hash(dict);
            var expected = Buffer.From("55CF8EC4CB42114B0C0A7FBB99EF44008602F815609DEBBB10EBC52608A828A8");
            Assert.Equal(expected.Parse(), actual.Parse());
        }

        [Fact]
        public void LegacyGtvHashVersionListTest()
        {
            var list = new List<List<int>>() { new List<int>() { 1 } };
            var hash = Gtv.Hash(list, 1);
            var expected = Buffer.From("67BB8D38054DB41A4B401F5971FF7560E48A730693E46371191ECEA9D7BD1E32");
            Assert.Equal(expected.Parse(), hash.Parse());
        }

        [Fact]
        public void GtvHashVersion2ListTest()
        {
            var list = new List<List<int>>() { new List<int>() { 1 } };
            var hash = Gtv.Hash(list);
            var expected = Buffer.From("082E13545DD8A1D4003143D17F781C9346BC500800592CD9B2D5D39DEDF05415");
            Assert.Equal(expected.Parse(), hash.Parse());
        }

        [Fact]
        public void LegacyGtvHashVersionDictTest()
        {
            var dict = new List<Dictionary<string, string>>() {
                new Dictionary<string, string>() { { "a", "b" }, { "c", "d" } },
            };
            var hash = Gtv.Hash(dict, 1);
            var expected = Buffer.From("891CDF10FF613A90899FF0FFE1A515D8ED74FE71E36249F0B6DD175EEC70805D");
            Assert.Equal(expected.Parse(), hash.Parse());
        }

        [Fact]
        public void GtvHashVersion2DictTest()
        {
            var dict = new List<Dictionary<string, string>>() {
                new Dictionary<string, string>() { { "a", "b" }, { "c", "d" } },
            };
            var hash = Gtv.Hash(dict);
            var expected = Buffer.From("9D2F6CFA72538E24584363ADA5882C2BE3F83D75AFF598D0009330DB22D961FF");
            Assert.Equal(expected.Parse(), hash.Parse());
        }

        [Fact]
        public void StructHashTest()
        {
            var obj = new MyBigStruct()
            {
                String = "foo",
                Buffer = ChromiaClient.Hash("bar"),
                Bool = true,
                Int = 1,
                Long = 1,
                Float = 1f,
                BigInt = 1,
                Enum = MyEnum.V2
            };
            var hash = Gtv.Hash(obj);
            var expected = Buffer.From("BA0D206C6ED3D9E751BB027A0C88BD84C70A123E44F12D7341594780CF695C9B");
            Assert.Equal(expected.Parse(), hash.Parse());
        }

        [Fact]
        public void ObjectHashTest()
        {
            var obj = new MyBigClass()
            {
                String = "foo",
                Buffer = ChromiaClient.Hash("bar"),
                Bool = true,
                Int = 1,
                Long = 1,
                Float = 1f,
                BigInt = 1,
                Enum = MyEnum.V2
            };
            var hash = Gtv.Hash(obj);
            var expected = Buffer.From("BA0D206C6ED3D9E751BB027A0C88BD84C70A123E44F12D7341594780CF695C9B");
            Assert.Equal(expected.Parse(), hash.Parse());
        }

        [Fact]
        public void MixedObjectHashTest()
        {
            var obj = new MyBigMixedClass("foo", ChromiaClient.Hash("bar"), true, 1, 1, 1f, 1, MyEnum.V2);
            var hash = Gtv.Hash(obj);
            var expected = Buffer.From("BA0D206C6ED3D9E751BB027A0C88BD84C70A123E44F12D7341594780CF695C9B");
            Assert.Equal(expected.Parse(), hash.Parse());
        }

        [Fact]
        public void MixedObjectInArrayHashTest()
        {
            var obj = new object[] { new MyBigMixedClass("foo", ChromiaClient.Hash("bar"), true, 1, 1, 1f, 1, MyEnum.V2) };
            var hash = Gtv.Hash(obj);
            var expected = Buffer.From("0DBD1C83F503CF5A32658E3E7223F7BA64936B178A25CC97C4247EB58E006B99");
            Assert.Equal(expected.Parse(), hash.Parse());
        }

        [Fact]
        public void NestedObjectHashTest()
        {
            var obj = new MyNestedStruct()
            {
                BigInt = 1,
                Struct = new MyStruct()
                {
                    A = "foo",
                    B = "bar"
                }
            };
            var hash = Gtv.Hash(obj);
            var expected = Buffer.From("9A78FF54FE4036856C550B4D94F58720958A23212F692CC1D31D70C2596744FE");
            Assert.Equal(expected.Parse(), hash.Parse());
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

        [Theory]
        [InlineData("[]")]
        [InlineData("[[]]")]
        [InlineData("[[[]]]")]
        [InlineData("[{}]")]
        [InlineData("[[{}]]")]
        public void LegacyEmptyObjectTest(string json)
        {
            var obj = JsonConvert.DeserializeObject(json);
            var hash = ChromiaClient.Hash(obj, 1);
            var actual = Buffer.From("46af9064f12528cad6a7c377204acd0ac38cdc6912903e7dab3703764c8dd5e5");
            Assert.Equal(hash, actual);
        }

        [Fact]
        public void LegacyEmptyListTest()
        {
            var obj = Array.Empty<object>();
            var hash = ChromiaClient.Hash(obj, 1);
            var actual = Buffer.From("46af9064f12528cad6a7c377204acd0ac38cdc6912903e7dab3703764c8dd5e5");
            Assert.Equal(hash, actual);
        }

        [Fact]
        public void EmptyNestedListTest()
        {
            var obj = new object[] { Array.Empty<object>(), Array.Empty<object>() };
            var hash = ChromiaClient.Hash(obj, 1);
            var actual = Buffer.From("8f0402234fe66da0b21f7c871cc2f3211fe8cee5af714bdcdb76bd9ef848b1fd");
            Assert.Equal(hash, actual);
        }

        [Fact]
        public void ListEqualToArrayHashTest()
        {
            var list = new List<int>() { int.MinValue, -1, 0, 1, int.MaxValue };
            var hash = ChromiaClient.Hash(list, 1);
            var actual = ChromiaClient.Hash(list.ToArray(), 1);
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