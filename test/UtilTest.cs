using Xunit;
using System;
using System.Text;

using Chromia.PostchainClient.GTX.ASN1Messages;

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
            const string privKey = "0101010101010101010101010101010101010101010101010101010101010101";
            const string pubKey = "031b84c5567b126440995d3ed5aaba0565d71e1834604819ff9c17f5e9d5dd078f";
            var verifiedPubKey = Util.VerifyKeyPair(privKey);

            Assert.Equal(pubKey, ASN1Util.ByteArrayToString(verifiedPubKey));
        }

        [Fact]
        public void CheckSignature(){
            const string expSignature = "bb432587f75316cad1a60494930917b2ed973e4aa5dd55e4c08dfe8eda6c017d3f1a40c8da1de454584828f43205dd0f670f8075309f546fb349c76d2e5bbb30";
            const string privKey = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            string pubKey = ASN1Util.ByteArrayToString(Util.VerifyKeyPair(privKey));
            var content = Encoding.UTF8.GetBytes("hello");
            var signature = Util.Sign(content, Util.HexStringToBuffer(privKey));
            
            Assert.Equal(ASN1Util.ByteArrayToString(signature), expSignature);
        }
    }
}