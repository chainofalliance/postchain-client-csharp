using Xunit;
using System.Text;

namespace Chromia.Postchain.Client.Tests.GTX
{
    public class UtilTest
    {
        [Fact]
        public void CheckKeyPairLength(){
            const int expPrivKeyLength = 32;
            const int expPubKeyLength = 33;

            var user = PostchainUtil.MakeKeyPair();
            var privKey = user["privKey"];
            var pubKey = user["pubKey"];

            Assert.Equal(privKey.Length, expPrivKeyLength);
            Assert.Equal(pubKey.Length, expPubKeyLength);
        }
    }
}