using Xunit;
using Chromia.PostchainClient.GTX.ASN1Messages;

namespace Chromia.PostchainClient.Tests.GTX
{
    public class GTXTest
    {
        [Fact]
        public void SimpleOperationTest(){
            var operationName = "TestOperation";
            var arg = new GTXValue();
            arg.Choice = GTXValueChoice.Integer;
            arg.Integer = 42;

            var gtxOperation = new GTXOperation();
            gtxOperation.OpName = operationName;
            gtxOperation.Args.Add(arg);

            var decoded = GTXOperation.Decode(gtxOperation.Encode());
            Assert.Equal(decoded.OpName, operationName);
            Assert.Equal(decoded.Args[0].Choice, arg.Choice);
            Assert.Equal(decoded.Args[0].Integer, arg.Integer);
        }

        [Fact]
        public void SimpleTransactionTest(){
            var blockhainID = ASN1Util.StringToByteArray("78967baa4768cbcef11c508326ffb13a956689fcb6dc3ba17f4b895cbb1577a3");
            var signer = ASN1Util.StringToByteArray("034ca0506ddf2328dc903c685f1638a9af33e572ee437867e7c4404bd21bf2adfe");
            var signature = ASN1Util.StringToByteArray("71a1fe694b7853209313cc09d2ba9f5115c63914eee33dd186a638b48fcbcd2464488e91787e9e9d5ef904b389d177e3b309f3951df44ead2047f0308832520c");

            var gtxTransaction = new GTXTransaction();
            gtxTransaction.BlockchainID = blockhainID;
            gtxTransaction.Signatures.Add(signature);
            gtxTransaction.Signers.Add(signer);

            var decoded = GTXTransaction.Decode(gtxTransaction.Encode());
            Assert.Equal(decoded.BlockchainID, blockhainID);
            Assert.Equal(decoded.Signers[0], signer);
            Assert.Equal(decoded.Signatures[0], signature);
        }
    }
}
