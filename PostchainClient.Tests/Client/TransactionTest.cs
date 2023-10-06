using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Chromia.Tests.Client
{
    [CollectionDefinition("Sequential", DisableParallelization = true)]
    public class TransactionTest : PrintableTest, IClassFixture<ResetableChromiaClientFixture>
    {
        private SignatureProvider Signer1 => StaticSignatureProvider.Signer1;
        private SignatureProvider Signer2 => StaticSignatureProvider.Signer2;

        private readonly ResetableChromiaClientFixture _fixture;
        private ChromiaClient Client => _fixture.Client;

        public TransactionTest(ITestOutputHelper output, ResetableChromiaClientFixture fixture) : base(output)
        {
            _fixture = fixture;
        }

        [Fact]
        public async void SimpleTransactionTest()
        {
            var tx1 = Client.TransactionBuilder()
                .AddOperation(new Operation("insert_city", "hamburg", 22222))
                .AddSignatureProvider(Signer1);
            var tx2 = Client.TransactionBuilder()
                .AddOperation(new Operation("insert_city", "hamburg", 22222))
                .AddSignatureProvider(Signer1);

            var response = await Client.SendUniqueTransaction(tx1);
            Assert.Equal(TransactionReceipt.ResponseStatus.Confirmed, response.Status);

            response = await Client.SendUniqueTransaction(tx1);
            Assert.Equal(TransactionReceipt.ResponseStatus.DoubleTx, response.Status);

            response = await Client.SendUniqueTransaction(tx2);
            Assert.Equal(TransactionReceipt.ResponseStatus.Rejected, response.Status);
        }

        [Fact]
        public async void DoubleTransactionTest()
        {
            var tx = Client.TransactionBuilder()
                .AddOperation(new Operation("insert_city", "hamburg", new Random().Next()))
                .AddSignatureProvider(Signer1);

            var response = await Client.SendTransaction(tx);
            Assert.Equal(TransactionReceipt.ResponseStatus.Confirmed, response.Status);

            response = await Client.SendTransaction(tx);
            Assert.Equal(TransactionReceipt.ResponseStatus.DoubleTx, response.Status);
            Assert.Equal("tx with hash already exists", response.RejectReason);
        }

        [Fact]
        public async void UnsignedTransactionTest()
        {
            var tx = Client.TransactionBuilder()
                .AddOperation(new Operation("insert_city", "hamburg", new Random().Next()));

            var response = await Client.SendTransaction(tx);
            Assert.Equal(TransactionReceipt.ResponseStatus.Confirmed, response.Status);
        }

        [Fact]
        public async void MultiSigTransactionTest()
        {
            var tx = Client.TransactionBuilder()
                .AddOperation(new Operation("insert_city", "hamburg", new Random().Next()))
                .AddSignatureProvider(Signer1)
                .AddSignatureProvider(Signer2);

            var response = await Client.SendUniqueTransaction(tx);
            Assert.Equal(TransactionReceipt.ResponseStatus.Confirmed, response.Status);
        }

        [Fact]
        public async void MultiSigRawTransactionTest()
        {
            var tx = Client.TransactionBuilder()
                .AddOperation(new Operation("insert_city", "hamburg", new Random().Next()))
                .AddNop()
                .AddSigner(Signer1.PubKey)
                .AddSigner(Signer2.PubKey);

            var signatures = new List<Signature> { tx.Sign(Signer1), tx.Sign(Signer2) };

            var signedTx = tx.Sign(signatures);
            var response = await Client.SendTransaction(signedTx);
            Assert.Equal(TransactionReceipt.ResponseStatus.Confirmed, response.Status);
        }

        [Fact]
        public void SignerWithoutSignatureTest()
        {
            var tx = Client.TransactionBuilder()
                .AddOperation(new Operation("insert_city", "hamburg", new Random().Next()))
                .AddSigner(Signer1.PubKey);

            Assert.ThrowsAsync<InvalidOperationException>(async () => { await Client.SendTransaction(tx); });
        }

        [Fact]
        public void InvalidSignatureTest()
        {
            var tx = Client.TransactionBuilder()
                .AddOperation(new Operation("insert_city", "hamburg", new Random().Next()));

            var sig = tx.Sign(Signer1);
            tx.AddNop();

            Assert.Throws<InvalidOperationException>(() => { tx.Sign(sig); });
        }

        [Fact]
        public async void DictTest()
        {
            var tx = Client.TransactionBuilder()
                .AddOperation(new Operation("test_map", new Dictionary<string, int>() { { "b", 42 }, { "a", 21 } }))
                .AddNop();

            var signed = tx.Sign();
            var response = await Client.SendTransaction(signed);
            Assert.Equal(TransactionReceipt.ResponseStatus.Confirmed, response.Status);
        }
    }
}
