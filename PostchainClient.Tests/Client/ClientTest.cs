using Xunit;
using Xunit.Abstractions;

namespace Chromia.Tests.Client
{
    public class ClientTest : PrintableTest
    {
        public ClientTest(ITestOutputHelper output) : base(output) { }

        [Fact]
        public async void GetBridTest()
        {
            var client = await ChromiaClient.Create("http://localhost:7750/", 0);
            Assert.Equal(ChromiaClientFixture.TestDappBrid, client.BlockchainRID);
        }


        [Fact]
        public async void GetBridRemoteTest()
        {
            var expected = Buffer.From("21041F6066E38D593E82D27C1B3843EB2D5FAC2AE75B9D32867CCAF3E30CCDCD");
            var client = await ChromiaClient.Create("https://lddgmoj5kh.execute-api.eu-north-1.amazonaws.com/demo", 0);
            Assert.Equal(expected, client.BlockchainRID);
        }
    }
}
