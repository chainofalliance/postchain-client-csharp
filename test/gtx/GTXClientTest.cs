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
            const string blockchainRID = "AC651CC730397A6880AD7695E73663720068532D7406F0BA0753C2F65A9AD169";

            var keyPair = Util.MakeKeyPair();
            var privKey = keyPair["privKey"];
            var pubKey = keyPair["pubKey"];

            var rest = new RESTClient("http://localhost:7740", blockchainRID);
            var gtx = new GTXClient(rest, blockchainRID);

            var req = gtx.NewTransaction(new byte[][] {pubKey});

            req.AddOperation("insert_city", "Hamburg", 22222);
            req.AddOperation("create_user", pubKey, "Peter");
            req.AddOperation("nop", new Random().Next());

            req.Sign(privKey, pubKey);

            var result = await req.PostAndWaitConfirmation();
            if (result.Error)
            {
                Console.WriteLine("Operation failed: " + result.ErrorMessage);
            }
            
            var queryResult = await gtx.Query<int>("get_city", ("name", "Hamburg"));
            if (queryResult.control.Error)
            {
                Console.WriteLine(queryResult.control.ErrorMessage);
            }
            else
            {
                int plz = queryResult.content;
                Console.WriteLine("PLZ Query: " + plz);
            }

            var queryResult2 = await gtx.Query<string>("get_user_pubkey", ("name", "Peter"));
            if (queryResult2.control.Error)
            {
                Console.WriteLine(queryResult2.control.ErrorMessage);
            }
            else
            {
                string queryPubkeyString = queryResult2.content;
                byte[] queryPubkey = Util.HexStringToBuffer(queryPubkeyString);
                Console.WriteLine("User Query: " + Util.ByteArrayToString(queryPubkey));
            }
        }
    }
}
