using System;
using Xunit;
using Newtonsoft.Json;

namespace Chromia.Postchain.Client.Tests
{
    public class GtxTest
    {
        [Fact]
        public void BufferToSignTest()
        {
            var keys1 = PostchainUtil.MakeKeyPair();
            var keys2 = PostchainUtil.MakeKeyPair();
            var gtx = new Gtx("abcdef1234567890abcdef1234567890");

            gtx.AddOperationToGtx("test", new object[]{"teststring"});
            
            gtx.AddSignerToGtx(keys1["pubKey"]);
            gtx.AddSignerToGtx(keys2["pubKey"]);

            gtx.Sign(keys1["privKey"], keys1["pubKey"]);
            gtx.Sign(keys2["privKey"], keys2["pubKey"]);
            
            var beforeSigs = gtx.Signatures;
            gtx.GetBufferToSign();
            var afterSigs = gtx.Signatures;

            Assert.Equal(beforeSigs, afterSigs);
        }
    }
}
