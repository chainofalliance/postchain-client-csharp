using Chromia.Encoding;
using Chromia.Tests.Client;
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
            var actual = (BigInteger)Gtv.Decode(Gtv.Encode(expected));
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

        [Fact]
        public void BigStructTest()
        {
            var expected = new MyBigStruct()
            {
                Bool = true,
                Buffer = Buffer.From("affe"),
                Float = 3.1415f,
                Int = 42,
                Long = -42,
                String = "foo",
                BigInt = BigInteger.MinusOne,
                Enum = MyEnum.V2
            };
            var hash = ChromiaClient.Hash(expected, 2);
            Console.WriteLine(hash);
        }

        [Fact]
        public void AuthDescTest()
        {
            var obj = new object[]
            {
                0,
                new object[]
                {
                    new object[] { "A", "T" },
                    Buffer.From("02897FAC9964FBDF97E6B83ECCBDE4A8D28729E0FB27059487D1B6B29F70B48767"),
                },
                null
            };
            Console.WriteLine(ChromiaClient.Hash(obj, 2));
        }

        [Fact]
        public void SanityTest()
        {
            var buffer = Buffer.From("a58201c1308201bda58201713082016da122042001ea98b5b20649ebd1c7d9d6c56c498dd029e4434937c0b604a2a181d2838a8ca582011c30820118a581a030819da20d0c0b6674342e66745f61757468a5818b308188a2420c4061386263633161626462383562616564613339306433353033383932343964643361336464633034653834383934376166393466643062393837333263303461a2420c4061386263633161626462383562616564613339306433353033383932343964643361336464633034653834383934376166393466643062393837333263303461a55c305aa2190c17636f612e494865726f2e7365745f65717569706d656e74a53d303ba2090c076865726f5f3136a42e302c302a0c0134a2250c2365717569706d656e745f726172655f74776f48616e645f677265617473776f72645f31a5153013a2050c036e6f70a50a3008a3060204d37a0278a5273025a1230421031134784145df267eddcaa545b6e5e56a58d13866c9818b31e2aaa1675b5e9aeaa5463044a1420440ed17a9af63ebfcc7b5c0fa6744ae587f056e8bb7b2fb2ea2981bfce9653e45cf4e86fbcc297b12f8270351769eec8644010e8ad664bb44f996711d46de549576");
            var obj = Gtv.Decode(buffer);
            Console.WriteLine(JsonConvert.SerializeObject(obj));
        }
    }
}
