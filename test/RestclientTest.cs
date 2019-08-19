using System;
using Xunit;
using Chromia.PostchainClient;

namespace Chromia.PostchainClient.Tests
{
    public class RestclientTest
    {
        private readonly Restclient _client;
        private readonly string _blockchainRID = "78967baa4768cbcef11c508326ffb13a956689fcb6dc3ba17f4b895cbb1577a3";

        public RestclientTest() {
            _client = new Restclient("http://localhost:7740", _blockchainRID);
        }

        public void serverExpectPost(string path, string requrestObject, int responseCode, string responseBody){
            
        }
    }
}
