using System;
using Xunit;

using Chromia.Postchain.Client.ASN1;

namespace Chromia.Postchain.Client.Tests.GTX
{
    public class ByteArrayTest : PrintableTest
    {
        [Fact]
        public void SimpleByteArrayTest()
        {
            var val = new GTXValue();
            val.Choice = GTXValueChoice.ByteArray;
            val.ByteArray = new byte[] { 0xaf, 0xfe, 0xca, 0xfe };

            if (PRINT_CONTENT)
            {
                var str = PostchainUtil.ByteArrayToString(val.Encode());
                System.Console.WriteLine("SimpleByteArrayTest: " + str.ToUpper());
            }

            var decoded = GTXValue.Decode(new AsnReader(val.Encode()));
            Assert.Equal(val.Choice, decoded.Choice);
            Assert.Equal(val.ByteArray, decoded.ByteArray);
            Assert.Equal(val, decoded);
        }

        [Fact]
        public void EmptyByteArrayTest()
        {
            var val = new GTXValue();
            val.Choice = GTXValueChoice.ByteArray;
            val.ByteArray = new byte[] { };

            if (PRINT_CONTENT)
            {
                var str = PostchainUtil.ByteArrayToString(val.Encode());
                System.Console.WriteLine("EmptyByteArrayTest: " + str.ToUpper());
            }

            var decoded = GTXValue.Decode(new AsnReader(val.Encode()));
            Assert.Equal(val.Choice, decoded.Choice);
            Assert.Equal(val.ByteArray, decoded.ByteArray);
            Assert.Equal(val, decoded);
        }
    }
}
