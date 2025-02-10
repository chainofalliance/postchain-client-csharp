using System;
using System.Collections.Generic;
using System.Numerics;
using Newtonsoft.Json;
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
        private ChromiaClient Client => _fixture.Client.SetHashVersion(1);

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
                .AddOperation(new Operation("insert_city", "hamburg", 2))
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

        [Fact]
        public async void BigIntTest()
        {
            var tx = Client.TransactionBuilder()
                .AddOperation(new Operation("test_bigint_op", BigInteger.One))
                .AddNop();

            var signed = tx.Sign();
            var response = await Client.SendTransaction(signed);
            Console.WriteLine(response.TransactionRID);
            Assert.Equal(TransactionReceipt.ResponseStatus.Confirmed, response.Status);
        }

        [Fact]
        public async void BigObjectTest()
        {
            var tx = Client.TransactionBuilder()
                .AddOperation(new Operation("test_bigobj_op", new MyBigMixedClass("foo", ChromiaClient.Hash("bar"), true, 1, 1, 1f, 1, MyEnum.V2)))
                .AddNop();

            var signed = tx.Sign();
            var response = await Client.SendTransaction(signed);
            Assert.Equal(TransactionReceipt.ResponseStatus.Confirmed, response.Status);
        }

        [Fact]
        public async void BigObjectArrayTest()
        {
            var tx = Client.TransactionBuilder()
                .AddOperation(new Operation("test_bigobj_array_op", new List<MyBigMixedClass>() {
                    new MyBigMixedClass("foo", ChromiaClient.Hash("bar"), true, 1, 1, 1f, 1, MyEnum.V2),
                    new MyBigMixedClass("foo2", ChromiaClient.Hash("bar2"), false, 2, 2, 2f, 2, MyEnum.V1)
                }))
                .AddNop();

            var signed = tx.Sign();
            var response = await Client.SendTransaction(signed);
            Assert.Equal(TransactionReceipt.ResponseStatus.Confirmed, response.Status);
        }

        [Fact]
        public async void NestedObjectArrayTest()
        {
            var tx = Client.TransactionBuilder()
                .AddOperation(new Operation("test_nested_obj_array_op", new List<MyNestedStruct>() {
                    new MyNestedStruct() {BigInt = BigInteger.One, Struct = new MyStruct() {A = "foo", B = "bar" } },
                    new MyNestedStruct() {BigInt = BigInteger.MinusOne, Struct = new MyStruct() {A = "foo2", B = "bar2" } },
                }))
                .AddNop();

            var signed = tx.Sign();
            var response = await Client.SendTransaction(signed);
            Assert.Equal(TransactionReceipt.ResponseStatus.Confirmed, response.Status);
        }
    }
}
