using Xunit;
using System;
using System.Collections.Generic;


namespace Chromia.Postchain.Client.Tests
{
    public class GTXClientTest
    {
        public GTXClient InitTest()
        {
            const string blockchainRID = "F7ACDB1458761FE3055E0C3C92DEEAF517D6F9382667D4B860C9C06A0205D26C";

            var rest = new RESTClient("http://localhost:7740/", blockchainRID);
            return new GTXClient(rest);
        }

        // [Fact]
        public async void StringTest(){
            var keyPair = PostchainUtil.MakeKeyPair();
            var privKey = keyPair["privKey"];
            var pubKey = keyPair["pubKey"];
            
            var gtx = InitTest();

            var req = gtx.NewTransaction(new byte[][] {pubKey});

            // req.AddOperation("send_string", null);
            req.AddOperation("send_string", "");
            req.AddOperation("send_string", "a");
            req.AddOperation("send_string", new string('a', 128));
            // req.AddOperation("send_string", new string('a', 1000000));


            req.AddOperation("nop", new Random().Next());

            req.Sign(privKey, pubKey);

            var result = await req.PostAndWaitConfirmation();
            if (result.Error)
            {
                Console.WriteLine("Error " + result.ErrorMessage);
            }
        }

        // [Fact]
        public async void IntegerTest(){
            var keyPair = PostchainUtil.MakeKeyPair();
            var privKey = keyPair["privKey"];
            var pubKey = keyPair["pubKey"];

            var gtx = InitTest();

            var req = gtx.NewTransaction(new byte[][] {pubKey});

            req.AddOperation("send_timestamp", -127);
            req.AddOperation("send_timestamp", -128);
            req.AddOperation("send_timestamp", -129);
            req.AddOperation("send_timestamp", -130);
            req.AddOperation("send_timestamp", -255);
            req.AddOperation("send_timestamp", -256);
            req.AddOperation("send_timestamp", System.Byte.MinValue);
            req.AddOperation("send_timestamp", System.Byte.MaxValue);

            req.AddOperation("send_timestamp", System.SByte.MinValue);
            req.AddOperation("send_timestamp", System.SByte.MaxValue);

            req.AddOperation("send_timestamp", System.UInt16.MinValue);
            req.AddOperation("send_timestamp", System.UInt16.MaxValue);

            req.AddOperation("send_timestamp", System.UInt32.MinValue);
            req.AddOperation("send_timestamp", System.UInt32.MaxValue);

            req.AddOperation("send_timestamp", System.Int16.MinValue);
            req.AddOperation("send_timestamp", System.Int16.MaxValue);

            req.AddOperation("send_timestamp", System.Int32.MinValue);
            req.AddOperation("send_timestamp", System.Int32.MaxValue);

            req.AddOperation("send_timestamp", System.Int64.MinValue);
            req.AddOperation("send_timestamp", System.Int64.MaxValue);

            req.AddOperation("nop", new Random().Next());

            req.Sign(privKey, pubKey);

            var result = await req.PostAndWaitConfirmation();
            if (result.Error)
            {
                Console.WriteLine("Error " + result.ErrorMessage);
            }
        }

        // [Fact]
        public async void QueryTest(){
            var keyPair = PostchainUtil.MakeKeyPair();
            var privKey = keyPair["privKey"];
            var pubKey = keyPair["pubKey"];

            var gtx = InitTest();

            var ret = await gtx.Query<long>("get_timestamp");
            if (ret.control.Error)
            {
                Console.WriteLine("Error " + ret.control.ErrorMessage);
            }
            else
            {
                Console.WriteLine("Success: " + ret.content);
            }
        }

        // [Fact]
        public async void ChainIDTest(){
            var rest = new RESTClient("http://localhost:7740/");

            var ret = await rest.InitializeBRIDFromChainID(0);
            if (ret.Error)
            {
                Console.WriteLine(ret.ErrorMessage);
            }
            else
            {
                Console.WriteLine("Success");
            }
        }

        [Fact]
        public async void DemoTest()
        {
            var keyPair = PostchainUtil.MakeKeyPair();
            var privKey = keyPair["privKey"];
            var pubKey = keyPair["pubKey"];

            // The lower-level client that can be used for any
            // postchain client messages. It only handles binary data.
            var rest = new RESTClient("http://localhost:7740/");

            // Instead of updateing the BRID each time the Rell code is compiled,
            // the blockchain can now be accessed throug its chain_id from the 
            // run.xml file (<chain name="MyCahin" iid="0">)
            var initResult = await rest.InitializeBRIDFromChainID(0);
            if (initResult.Error)
            {
                Console.WriteLine("Cannot connect to blockchain!");
                return;
            }

            // Create an instance of the higher-level gtx client. It will
            // use the rest client instance
            var gtx = new GTXClient(rest);

            // Start a new request. A request instance is created.
            // The public keys are the keys that must sign the request
            // before sending it to postchain. Can be empty.

            var req = gtx.NewTransaction(new byte[][] {pubKey});

            req.AddOperation("insert_city", "Hamburg", 22222);
            req.AddOperation("create_user", pubKey, "Peter");

            // Since transactions with the same operations will result in the same txid,
            // transactions can contain "nop" operations. This is needed to satisfy
            // the unique txid constraint of the postchain. 
            req.AddOperation("nop", new Random().Next());

            req.Sign(privKey, pubKey);

            var result = await req.PostAndWaitConfirmation();
            if (result.Error)
            {
                Console.WriteLine("Operation failed: " + result.ErrorMessage);
            }

            // The expected return type has to be passed to the query function. This
            // also works with complex types (i.e. your own struct as well as lists).
            // The returned tuple will consist of (content, control). The content is of
            // the type you pass the function. The control struct contains an error flag
            // as well as the error message.
            var queryResult = await gtx.Query<int>("get_city", ("name", "Hamburg"));
            if (queryResult.control.Error)
            {
                Console.WriteLine(queryResult.control.ErrorMessage);
            }
            else
            {
                int plz = queryResult.content;
                Console.WriteLine("ZIP Query: " + plz);
            }

            // Same as above with the exception that byte arrays will be returned as strings.
            // To convert it to a byte array, use the util function Util.HexStringToBuffer() 
            // in the Chromia.Postchain.Client.GTX namespace.
            var queryResult2 = await gtx.Query<string>("get_user_pubkey", ("name", "Peter"));
            if (queryResult2.control.Error)
            {
                Console.WriteLine(queryResult2.control.ErrorMessage);
            }
            else
            {
                string queryPubkeyString = queryResult2.content;
                byte[] queryPubkey = PostchainUtil.HexStringToBuffer(queryPubkeyString);
                Console.WriteLine("User Query: " + PostchainUtil.ByteArrayToString(queryPubkey));
            }
        }
    }
}
