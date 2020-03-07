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

        [Fact]
        public void CheckKeyPairAuthenticity(){           
            const string privKey = "0101010101010101010101010101010101010101010101010101010101010101";
            const string pubKey = "031b84c5567b126440995d3ed5aaba0565d71e1834604819ff9c17f5e9d5dd078f";
            var verifiedPubKey = PostchainUtil.VerifyKeyPair(privKey);

            Assert.Equal(pubKey, PostchainUtil.ByteArrayToString(verifiedPubKey));
        }
    }
}