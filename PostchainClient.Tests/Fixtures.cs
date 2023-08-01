using Chromia.Encoding;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Chromia.Tests
{
    public class ChromiaClientFixture : IAsyncLifetime
    {
        public static Buffer TestDappBrid = Buffer.From("8BFFC5F6A9271510237AAF4D04411773D4CDFD7A43834B28D908D6C5F7C64C27");
        public ChromiaClient Client { get; private set; }

        public async Task InitializeAsync()
        {
            Client = await ChromiaClient.Create("http://localhost:7740/", TestDappBrid);
            Client.SetAttemptsPerEndpoint(1);
        }

        public async Task DisposeAsync()
        {
            await Task.FromResult(0);
        }
    }
    public class ResetableChromiaClientFixture : IAsyncLifetime
    {
        public static Buffer TestDappBrid = Buffer.From("8BFFC5F6A9271510237AAF4D04411773D4CDFD7A43834B28D908D6C5F7C64C27");
        public ChromiaClient Client { get; private set; }

        public async Task InitializeAsync()
        {
            Client = await ChromiaClient.Create("http://localhost:7740/", TestDappBrid);
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
