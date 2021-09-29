using System;
using Xunit;

using Chromia.Postchain.Client.ASN1;

namespace Chromia.Postchain.Client.Tests.GTX
{
    public class ArrayTest : PrintableTest
    {
        [Fact]
        public void SimpleArrayTest()
        {
            var val = new GTXValue();
            val.Choice = GTXValueChoice.Array;
            val.Array = new System.Collections.Generic.List<GTXValue>();

            var innerVal = new GTXValue();
            innerVal.Choice = GTXValueChoice.String;
            innerVal.String = "test";

            val.Array.Add(innerVal);

            if (PRINT_CONTENT)
            {
                var str = PostchainUtil.ByteArrayToString(val.Encode());
                System.Console.WriteLine("SimpleArrayTest: " + str.ToUpper());
            }

            var decoded = GTXValue.Decode(new AsnReader(val.Encode()));
            Assert.Equal(val.Choice, decoded.Choice);
            Assert.Equal(val.Array, decoded.Array);
            Assert.Equal(val, decoded);
        }

        [Fact]
        public void EmptyArrayTest()
        {
            var val = new GTXValue();
            val.Choice = GTXValueChoice.Array;
            val.Array = new System.Collections.Generic.List<GTXValue>();

            if (PRINT_CONTENT)
            {
                var str = PostchainUtil.ByteArrayToString(val.Encode());
                System.Console.WriteLine("EmptyArrayTest: " + str.ToUpper());
            }

            var decoded = GTXValue.Decode(new AsnReader(val.Encode()));
            Assert.Equal(val.Choice, decoded.Choice);
            Assert.Equal(val.Array, decoded.Array);
            Assert.Equal(val, decoded);
        }

        [Fact]
        public void ArrayInArrayTest()
        {
            var val = new GTXValue();
            val.Choice = GTXValueChoice.Array;
            val.Array = new System.Collections.Generic.List<GTXValue>();

            var innerVal = new GTXValue();
            innerVal.Choice = GTXValueChoice.Array;
            innerVal.Array = new System.Collections.Generic.List<GTXValue>();

            var innerInnerVal = new GTXValue();
            innerInnerVal.Choice = GTXValueChoice.Integer;
            innerInnerVal.Integer = Int64.MinValue;
            innerVal.Array.Add(innerInnerVal);

            val.Array.Add(innerVal);

            if (PRINT_CONTENT)
            {
                var str = PostchainUtil.ByteArrayToString(val.Encode());
                System.Console.WriteLine("ArrayInArrayTest: " + str.ToUpper());
            }

            var decoded = GTXValue.Decode(new AsnReader(val.Encode()));
            Assert.Equal(val.Choice, decoded.Choice);
            Assert.Equal(val.Array, decoded.Array);
            Assert.Equal(val, decoded);
        }

        [Fact]
        public void FullArrayTest()
        {
            var val = new GTXValue();
            val.Choice = GTXValueChoice.Array;
            val.Array = new System.Collections.Generic.List<GTXValue>();

            var innerVal1 = new GTXValue();
            innerVal1.Choice = GTXValueChoice.String;
            innerVal1.String = "test";
            val.Array.Add(innerVal1);

            var innerVal2 = new GTXValue();
            innerVal2.Choice = GTXValueChoice.Null;
            val.Array.Add(innerVal2);

            var innerVal3 = new GTXValue();
            innerVal3.Choice = GTXValueChoice.Integer;
            innerVal3.Integer = Int64.MaxValue;
            val.Array.Add(innerVal3);

            var innerVal4 = new GTXValue();
            innerVal4.Choice = GTXValueChoice.ByteArray;
            innerVal4.ByteArray = new byte[] { 0xde, 0xad, 0xbe, 0xef };
            val.Array.Add(innerVal4);

            var innerVal5 = new GTXValue();
            innerVal5.Choice = GTXValueChoice.Array;
            innerVal5.Array = new System.Collections.Generic.List<GTXValue>();
            val.Array.Add(innerVal5);

            if (PRINT_CONTENT)
            {
                var str = PostchainUtil.ByteArrayToString(val.Encode());
                System.Console.WriteLine("FullArrayTest: " + str.ToUpper());
            }

            var decoded = GTXValue.Decode(new AsnReader(val.Encode()));
            Assert.Equal(val.Choice, decoded.Choice);
            Assert.Equal(val.Array, decoded.Array);
            Assert.Equal(val, decoded);
        }
    }
}
