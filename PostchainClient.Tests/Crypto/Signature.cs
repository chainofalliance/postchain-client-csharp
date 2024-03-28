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

        [Fact]
        public void SanityTest()
        {
            var buffer = Buffer.From("bd65d27b57447d7a01c9fa66f4d6a503bc97dafdcb9149ebe2fa0914b1220440");
            var signature = Buffer.From("1932264ea0183603780eb1e4e17a6f7a00874de829b28216985ddec95300f267044ed45011332bc4e2dc6f2a1109f7ee099d9fdbada32215cde326f9f7a0a00b");
            var pubkey = Buffer.From("031134784145df267eddcaa545b6e5e56a58d13866c9818b31e2aaa1675b5e9aea");

            var sig = new Signature(pubkey, signature);
            Assert.True(SignatureProvider.Verify(sig, buffer));
        }
    }
}
