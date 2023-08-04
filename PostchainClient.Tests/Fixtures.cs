using Chromia.Encoding;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Chromia.Tests
{
    public class ChromiaClientFixture : IAsyncLifetime
    {
        public static Buffer TestDappBrid = Buffer.From("493AB3DCF274B382BCE618C08E5C8D0F5C294C2A6718C28EF9273186A471F819");
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
        public ChromiaClient Client { get; private set; }

        public async Task InitializeAsync()
        {
            Client = await ChromiaClient.Create("http://localhost:7740/", ChromiaClientFixture.TestDappBrid);
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
