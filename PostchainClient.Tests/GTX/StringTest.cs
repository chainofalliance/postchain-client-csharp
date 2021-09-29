using Xunit;

using Chromia.Postchain.Client.ASN1;

namespace Chromia.Postchain.Client.Tests.GTX
{
    public class StringTest : PrintableTest
    {
        [Fact]
        public void SimpleStringTest()
        {
            var val = new GTXValue();
            val.Choice = GTXValueChoice.String;
            val.String = "test";

            if (PRINT_CONTENT)
            {
                var str = PostchainUtil.ByteArrayToString(val.Encode());
                System.Console.WriteLine("SimpleStringTest: " + str.ToUpper());
            }

            var decoded = GTXValue.Decode(new AsnReader(val.Encode()));
            Assert.Equal(val.Choice, decoded.Choice);
            Assert.Equal(val.String, decoded.String);
            Assert.Equal(val, decoded);
        }

        [Fact]
        public void EmptyStringTest()
        {
            var val = new GTXValue();
            val.Choice = GTXValueChoice.String;
            val.String = "";

            if (PRINT_CONTENT)
            {
                var str = PostchainUtil.ByteArrayToString(val.Encode());
                System.Console.WriteLine("EmptyStringTest: " + str.ToUpper());
            }

            var decoded = GTXValue.Decode(new AsnReader(val.Encode()));
            Assert.Equal(val.Choice, decoded.Choice);
            Assert.Equal(val.String, decoded.String);
            Assert.Equal(val, decoded);
        }

        [Fact]
        public void LongStringTest()
        {
            var val = new GTXValue();
            val.Choice = GTXValueChoice.String;
            val.String = new string('x', 2048);

            if (PRINT_CONTENT)
            {
                var str = PostchainUtil.ByteArrayToString(val.Encode());
                System.Console.WriteLine("LongStringTest: " + str.ToUpper());
            }

            var decoded = GTXValue.Decode(new AsnReader(val.Encode()));
            Assert.Equal(val.Choice, decoded.Choice);
            Assert.Equal(val.String, decoded.String);
            Assert.Equal(val, decoded);
        }
    }
}
