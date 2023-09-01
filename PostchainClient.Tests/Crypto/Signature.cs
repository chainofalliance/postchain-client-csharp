using System;
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

        [Fact]
        public void KeyPairTest()
        {
            var privkey = Buffer.From("1234123412341234123412341234123412341234123412341234123412341235");
            var kp = new KeyPair(privkey);
            Assert.Equal(privkey, kp.PrivKey);
        }
    }
}
