using Xunit;

using Chromia.Postchain.Client.ASN1;

namespace Chromia.Postchain.Client.Tests.GTX
{
    public class NullTest : PrintableTest
    {
        [Fact]
        public void SimpleNullTest()
        {
            var val = new GTXValue();
            val.Choice = GTXValueChoice.Null;

            if (PRINT_CONTENT)
            {
                var str = PostchainUtil.ByteArrayToString(val.Encode());
                System.Console.WriteLine("SimpleNullTest: " + str.ToUpper());
            }

            var decoded = GTXValue.Decode(new AsnReader(val.Encode()));
            Assert.Equal(val.Choice, decoded.Choice);
            Assert.Equal(val, decoded);
        }
    }
}
