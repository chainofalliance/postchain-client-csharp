using System.Threading.Tasks;
using Xunit;

namespace Chromia.Tests
{
    public class ChromiaClientFixture : IAsyncLifetime
    {
        public static Buffer TestDappBrid = Buffer.From("454CCD42954A810801FAD9885A9A6E65BC1EAAE4F0E23D6D416E8718344EBDB4");
        public ChromiaClient Client { get; private set; }

        public async Task InitializeAsync()
        {
            Client = await ChromiaClient.Create("http://localhost:7750/", TestDappBrid);
            Client.SetAttemptsPerEndpoint(1)
                .SetPollingRetries(1);
        }

        public async Task DisposeAsync()
        {
            await Task.FromResult(0);
        }
    }
    public class ResetableChromiaClientFixture : IAsyncLifetime
    {
        public ChromiaClient Client { get; private set; }

        public async Task InitializeAsync()
        {
            Client = await ChromiaClient.Create("http://localhost:7750/", ChromiaClientFixture.TestDappBrid);
            Client.SetAttemptsPerEndpoint(1);
            await Client.SendUniqueTransaction(new Operation("reset"));
        }

        public async Task DisposeAsync()
        {
            await Task.FromResult(0);
        }
    }

    public static class StaticSignatureProvider
    {
        public static SignatureProvider Signer1 => GetSignatureProvider(1);
        public static SignatureProvider Signer2 => GetSignatureProvider(2);
        public static SignatureProvider Signer3 => GetSignatureProvider(3);
        public static SignatureProvider Signer4 => GetSignatureProvider(4);
        public static SignatureProvider Signer5 => GetSignatureProvider(5);

        public static SignatureProvider GetSignatureProvider(int nr)
        {
            Buffer privKey = Buffer.Repeat((char)nr, 32);
            return SignatureProvider.Create(privKey);
        }
    }
}
