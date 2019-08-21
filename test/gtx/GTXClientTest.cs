using Xunit;
using Chromia.PostchainClient.GTX.ASN1Messages;
using Chromia.PostchainClient.GTX;
using System;

namespace Chromia.PostchainClient.Tests.GTX
{
    public class GTXClientTest
    {
        [Fact]
        public void FullClientTest(){
            const string blockchainRID = "78967baa4768cbcef11c508326ffb13a956689fcb6dc3ba17f4b895cbb1577a3";

            string signerPrivKeyA = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            string signerPubKeyA = ASN1Util.ByteArrayToString(Util.VerifyKeyPair(signerPrivKeyA));

            string signerPrivKeyB = "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb";
            string signerPubKeyB = ASN1Util.ByteArrayToString(Util.VerifyKeyPair(signerPrivKeyB));

            // The lower-level client that can be used for any
            // postchain client messages. It only handles binary data.
            var rest = new RESTClient("http://localhost:7740", blockchainRID);

            // Create an instance of the higher-level gtx client. It will
            // use the rest client instance and it will allow calls to functions
            // fun1 and fun2. The backend implementation in Postchain must
            // provide those functions.
            var gtx = new GTXClient(rest, blockchainRID);

            // Start a new request. A request instance is created.
            // The public keys are the keys that must sign the request
            // before sending it to postchain. Can be empty.
            var req = gtx.NewTransaction(new byte[][] {Util.HexStringToBuffer(signerPubKeyA)});

            // call fun1 with three arguments: a string, an array and a Buffer
            dynamic nopVal = 42;
            req.AddOperation("nop", nopVal);

            req.Sign(Util.HexStringToBuffer(signerPrivKeyA), Util.HexStringToBuffer(signerPubKeyA));

            var txRID = req.GetTxRID();

            var promise = req.PostAndWaitConfirmation();
            promise.Then(result => Console.WriteLine(result));
        }
    }
}
