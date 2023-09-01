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
    }
}
