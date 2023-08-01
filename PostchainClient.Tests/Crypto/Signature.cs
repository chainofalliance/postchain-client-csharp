using Xunit;
using Xunit.Abstractions;

namespace Chromia.Tests.Crypto
{
    public class SignatureTest : PrintableTest
    {
        public SignatureTest(ITestOutputHelper output) : base(output) { }

        [Fact]
        public void SignatureProviderTest()
        {
            var signer = SignatureProvider.Create();
            var buffer = ChromiaClientFixture.TestDappBrid;
            var sig = signer.Sign(buffer);
            Assert.True(SignatureProvider.Verify(sig, buffer));
        }
    }
}
