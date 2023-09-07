using Chromia.Encoding;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Chromia.Tests.Client
{
    struct MyStruct
    {
        [JsonProperty("a")]
        public string A;
        [JsonProperty("b")]
        public string B;
    }

    struct MyStructReverse
    {
        [JsonProperty("b")]
        public string B;
        [JsonProperty("a")]
        public string A;
    }

    struct MyStructQueryObject : IGtvSerializable
    {
        [JsonProperty("s")]
        public MyStruct Struct;
    }

    struct MyNestedStruct
    {
        [JsonProperty("n")]
        public BigInteger BigInt;
        [JsonProperty("my_struct")]
        public MyStruct Struct;
    }

    struct MyBigStruct
    {
        [JsonProperty("s")]
        public string String;
        [JsonProperty("ba")]
        public Buffer Buffer;
        [JsonProperty("b")]
        public bool Bool;
        [JsonProperty("i")]
        public int Int;
        [JsonProperty("l")]
        public long Long;
        [JsonProperty("f")]
        public float Float;
        [JsonProperty("n")]
        public BigInteger BigInt;

    }


    struct CityStruct : IGtvSerializable
    {
        [JsonProperty("name")]
        public string Name;
        [JsonProperty("zip")]
        public int Zip;
    }
}
