using Xunit;
using Chromia.Postchain.Client.GTX;
using System;
using System.Collections.Generic;
using Chromia.Postchain.Client.ASN1;


namespace Chromia.Postchain.Client.Tests.GTX
{
    public class GTXClientTest
    {
        public GTXClient InitTest()
        {
            const string blockchainRID = "9D83AB17ECF5A021276240C000DA139916DB821826CA698422675B67830F34F1";

            var rest = new RESTClient("http://localhost:7740", blockchainRID);
            return new GTXClient(rest, blockchainRID);
        }

        //[Fact]
        public async void StringTest(){
            var keyPair = Util.MakeKeyPair();
            var privKey = keyPair["privKey"];
            var pubKey = keyPair["pubKey"];
            
            var gtx = InitTest();

            var req = gtx.NewTransaction(new byte[][] {pubKey});

            req.AddOperation("send_string", null);
            req.AddOperation("send_string", "");
            req.AddOperation("send_string", "a");
            req.AddOperation("send_string", new string('a', 128));
            req.AddOperation("send_string", new string('a', 1000000));


            req.AddOperation("nop", new Random().Next());

            req.Sign(privKey, pubKey);

            var result = await req.PostAndWaitConfirmation();
            Assert.False(result.Error);
        }

        [Fact]
        public async void IntegerTest(){
            var keyPair = Util.MakeKeyPair();
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

            Console.WriteLine(req.Encode());

            var result = await req.PostAndWaitConfirmation();
            Assert.False(result.Error);
        }
    }
}
