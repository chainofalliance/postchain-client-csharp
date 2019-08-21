using Xunit;
using System;

namespace Chromia.PostchainClient.Tests.GTX
{
    public class UtilTest
    {
        [Fact]
        public void CheckKeyPairLength(){
            const int expPrivKeyLength = 32;
            const int expPubKeyLength = 33;

            var user = Util.MakeKeyPair();
            var privKey = user["privKey"];
            var pubKey = user["pubKey"];

            Assert.Equal(privKey.Length, expPrivKeyLength);
            Assert.Equal(pubKey.Length, expPubKeyLength);
        }

        [Fact]
        public void CheckKeyPairAuthenticity(){           
            //const privKey = "0101010101010101010101010101010101010101010101010101010101010101";
        }
    }
}