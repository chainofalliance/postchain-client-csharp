using Xunit;
using Chromia.Postchain.Client.GTX;
using System;
using System.Collections.Generic;

namespace Chromia.Postchain.Client.Tests.GTX
{
    public class GTXClientTest
    {
        [Fact]
        public async void FullClientTest(){
            // Default RID from eclipse plugin
            const string blockchainRID = "0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF";

            var keyPair = Util.MakeKeyPair();
            var privKey = keyPair["privKey"];
            var pubKey = keyPair["pubKey"];

            // The lower-level client that can be used for any
            // postchain client messages. It only handles binary data.
            var rest = new RESTClient("http://localhost:7740", blockchainRID);

            // Create an instance of the higher-level gtx client. It will
            // use the rest client instance
            var gtx = new GTXClient(rest, blockchainRID);

            // Start a new request. A request instance is created.
            // The public keys are the keys that must sign the request
            // before sending it to postchain. Can be empty.
            
            var req = gtx.NewTransaction(new byte[][] {pubKey});

            var dict = new Dictionary<string,dynamic>();
            dict.Add("name", "Hamburg");
            req.AddOperation("insert_city_dict", dict);
            req.AddOperation("create_user", pubKey, "Peter");
            req.AddOperation("nop", 1000);

            req.Sign(privKey, pubKey);

            var result = await req.PostAndWaitConfirmation();
            Console.WriteLine("Operation: " + result);
            
            result = await gtx.Query("get_city", ("name", "Hamburg"));
            Console.WriteLine("Query: " + result);

            result = await gtx.Query("get_user_name", ("pubkey", pubKey));
            Console.WriteLine("Query2: " + result);
        }
    }
}
