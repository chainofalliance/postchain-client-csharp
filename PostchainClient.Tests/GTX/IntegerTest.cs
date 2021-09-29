using System;
using Xunit;

using Chromia.Postchain.Client.ASN1;

namespace Chromia.Postchain.Client.Tests.GTX
{
    public class IntegerTest : PrintableTest
    {
        [Fact]
        public void SimpleIntegerTest()
        {
            var val = new GTXValue();
            val.Choice = GTXValueChoice.Integer;
            val.Integer = 1337;

            if (PRINT_CONTENT)
            {
                var str = PostchainUtil.ByteArrayToString(val.Encode());
                System.Console.WriteLine("SimpleIntegerTest: " + str.ToUpper());
            }

            var decoded = GTXValue.Decode(new AsnReader(val.Encode()));
            Assert.Equal(val.Choice, decoded.Choice);
            Assert.Equal(val.Integer, decoded.Integer);
            Assert.Equal(val, decoded);
        }

        [Fact]
        public void NegativeIntegerTest()
        {
            var val = new GTXValue();
            val.Choice = GTXValueChoice.Integer;
            val.Integer = -1337;

            if (PRINT_CONTENT)
            {
                var str = PostchainUtil.ByteArrayToString(val.Encode());
                System.Console.WriteLine("NegativeIntegerTest: " + str.ToUpper());
            }

            var decoded = GTXValue.Decode(new AsnReader(val.Encode()));
            Assert.Equal(val.Choice, decoded.Choice);
            Assert.Equal(val.Integer, decoded.Integer);
            Assert.Equal(val, decoded);
        }

        [Fact]
        public void ZeroIntegerTest()
        {
            var val = new GTXValue();
            val.Choice = GTXValueChoice.Integer;
            val.Integer = 0;

            if (PRINT_CONTENT)
            {
                var str = PostchainUtil.ByteArrayToString(val.Encode());
                System.Console.WriteLine("ZeroIntegerTest: " + str.ToUpper());
            }

            var decoded = GTXValue.Decode(new AsnReader(val.Encode()));
            Assert.Equal(val.Choice, decoded.Choice);
            Assert.Equal(val.Integer, decoded.Integer);
            Assert.Equal(val, decoded);
        }

        [Fact]
        public void MaxIntegerTest()
        {
            var val = new GTXValue();
            val.Choice = GTXValueChoice.Integer;
            val.Integer = Int64.MaxValue;

            if (PRINT_CONTENT)
            {
                var str = PostchainUtil.ByteArrayToString(val.Encode());
                System.Console.WriteLine("MaxIntegerTest: " + str.ToUpper());
            }

            var decoded = GTXValue.Decode(new AsnReader(val.Encode()));
            Assert.Equal(val.Choice, decoded.Choice);
            Assert.Equal(val.Integer, decoded.Integer);
            Assert.Equal(val, decoded);
        }

        [Fact]
        public void MinIntegerTest()
        {
            var val = new GTXValue();
            val.Choice = GTXValueChoice.Integer;
            val.Integer = Int64.MinValue;

            if (PRINT_CONTENT)
            {
                var str = PostchainUtil.ByteArrayToString(val.Encode());
                System.Console.WriteLine("MinIntegerTest: " + str.ToUpper());
            }

            var decoded = GTXValue.Decode(new AsnReader(val.Encode()));
            Assert.Equal(val.Choice, decoded.Choice);
            Assert.Equal(val.Integer, decoded.Integer);
            Assert.Equal(val, decoded);
        }
    }
}
