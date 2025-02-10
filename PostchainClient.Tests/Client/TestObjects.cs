using Chromia.Encoding;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace Chromia.Tests.Client
{
    [PostchainSerializable]
    struct MyStruct
    {
        [PostchainProperty("a")]
        public string A;
        [PostchainProperty("b")]
        public string B;

        public override bool Equals([NotNullWhen(true)] object obj)
        {
            if (obj == null || !(obj is MyStruct))
                return false;

            var other = (MyStruct)obj;
            return A == other.A && B == other.B;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(A, B);
        }
    }

    [PostchainSerializable]
    struct MyStructReverse
    {
        [PostchainProperty("b")]
        public string B;
        [PostchainProperty("a")]
        public string A;
    }

    [PostchainSerializable]
    struct MyStructQueryObject
    {
        [PostchainProperty("s")]
        public MyStruct Struct;
    }

    [PostchainSerializable]
    struct MyNestedStruct
    {
        [PostchainProperty("n")]
        public BigInteger BigInt;
        [PostchainProperty("my_struct")]
        public MyStruct Struct;

        public override bool Equals([NotNullWhen(true)] object obj)
        {
            if (obj == null || !(obj is MyNestedStruct))
                return false;

            var other = (MyNestedStruct)obj;
            return BigInt == other.BigInt && Struct.Equals(other.Struct);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(BigInt, Struct);
        }
    }

    [PostchainSerializable]
    struct MyBigStruct
    {
        [PostchainProperty("s")]
        public string String;
        [PostchainProperty("ba")]
        public Buffer Buffer;
        [PostchainProperty("b")]
        public bool Bool;
        [PostchainProperty("i")]
        public int Int;
        [PostchainProperty("l")]
        public long Long;
        [PostchainProperty("f")]
        public float Float;
        [PostchainProperty("n")]
        public BigInteger BigInt;
        [PostchainProperty("e")]
        public MyEnum Enum;
    }

    [PostchainSerializable]
    class MyBigClass
    {
        [PostchainProperty("s")]
        public string String;
        [PostchainProperty("ba")]
        public Buffer Buffer;
        [PostchainProperty("b")]
        public bool Bool;
        [PostchainProperty("i")]
        public int Int;
        [PostchainProperty("l")]
        public long Long;
        [PostchainProperty("f")]
        public float Float;
        [PostchainProperty("n")]
        public BigInteger BigInt;
        [PostchainProperty("e")]
        public MyEnum Enum;
    }

    [PostchainSerializable]
    class MyBigMixedClass
    {
        [PostchainProperty("s", 1)]
        public string String;
        [PostchainProperty("ba", 2)]
        private Buffer Buffer;
        [PostchainProperty("b", 3)]
        public bool Bool { get; }
        [PostchainProperty("i", 4)]
        public int Int { get; private set; }
        [PostchainProperty("l", 5)]
        private long Long { get; }
        [PostchainProperty("f", 6)]
        private float Float { get; set; }
        [PostchainProperty("n", 7)]
        public BigInteger BigInt;
        [PostchainProperty("e", 8)]
        public MyEnum Enum;


        public MyBigMixedClass(string s, Buffer ba, bool b, int i, long l, float f, BigInteger n, MyEnum e)
        {
            String = s;
            Buffer = ba;
            Bool = b;
            Int = i;
            Long = l;
            Float = f;
            BigInt = n;
            Enum = e;
        }
    }


    enum MyEnum
    {
        V1,
        V2,
    }

    [PostchainSerializable]
    struct CityStruct
    {
        [PostchainProperty("name")]
        public string Name;
        [PostchainProperty("zip")]
        public int Zip;

        public CityStruct(string name, int zip)
        {
            Name = name;
            Zip = zip;
        }
    }
}
