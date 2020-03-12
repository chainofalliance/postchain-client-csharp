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

        [Fact]
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
    }
}
