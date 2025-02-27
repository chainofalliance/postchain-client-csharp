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

        [Theory]
        [InlineData("6F97EE26541E95CC4F5CEE0DD2542506B45F9EB24B4CA2A85F1DB566FF94FBA3", 1)]
        [InlineData("DCE5D72ED7E1675291AFE7F9D649D898C8D3E7411E52882D03D1B3D240BDD91B", 2)]
        public async void GetHashVersionTest(string bridString, int expectedHashVersion)
        {
            var brid = Buffer.From(bridString);
            var client = await ChromiaClient.Create("https://node11.devnet1.chromia.dev:7740/", brid);
            Assert.Equal(expectedHashVersion, client.HashVersion);
        }

        [Theory]
        [InlineData("6F97EE26541E95CC4F5CEE0DD2542506B45F9EB24B4CA2A85F1DB566FF94FBA3", 1)]
        [InlineData("DCE5D72ED7E1675291AFE7F9D649D898C8D3E7411E52882D03D1B3D240BDD91B", 2)]
        public async void GetHashVersionFromDirectoryTest(string bridString, int expectedHashVersion)
        {
            var brid = Buffer.From(bridString);
            var client = await ChromiaClient.CreateFromDirectory("https://node8.devnet1.chromia.dev:7740/", brid);
            Assert.Equal(expectedHashVersion, client.HashVersion);
        }
    }
}
