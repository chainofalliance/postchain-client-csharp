using Xunit;
using Chromia.PostchainClient.GTX;
using System;
using System.Collections.Generic;

namespace Chromia.PostchainClient.Tests.GTX
{
    public class GTXClientTest
    {
        [Fact]
        public async void FullClientTest(){
            const string blockchainRID = "78967baa4768cbcef11c508326ffb13a956689fcb6dc3ba17f4b895cbb1577a3";

            string signerPrivKeyA = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            string signerPubKeyA = "02e5a018b3a2e155316109d9cdc5eab739759c0e07e0c00bf9fccb8237fe4d7f02";

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
            dynamic[] opVal = {"Hamburg", 223232};
            req.AddOperation("insert_city", opVal);

            req.Sign(Util.StringToByteArray(signerPrivKeyA), Util.HexStringToBuffer(signerPubKeyA));

            var result = await req.PostAndWaitConfirmation();
            Console.WriteLine("Operation: " + result);
            
            var queryObject = new List<dynamic> {("name", "Hamburg")};
            result = await gtx.Query("get_city", queryObject);
            Console.WriteLine("Query: " + result);
            
        }
    }
}
