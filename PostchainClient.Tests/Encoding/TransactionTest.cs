using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

namespace Chromia.Tests.Encoding
{
    public class TransactionTest : PrintableTest
    {
        private SignatureProvider Signer1 => StaticSignatureProvider.Signer1;
        private SignatureProvider Signer2 => StaticSignatureProvider.Signer2;

        public TransactionTest(ITestOutputHelper output) : base(output) { }

        private static readonly Buffer BlockchainRID = Buffer.From("7848422629D30011403E100FCB910A9A364677A9E84D05A66B6320D0E261EF71");


        [Fact]
        public void EmptyTransactionTest()
        {
            var expected = Transaction.Build(BlockchainRID);
            var actual = Transaction.Decode(expected.Sign().SignedHash);
            Assert.Equal(expected.TransactionRID(), actual.TransactionRID);
        }

        [Fact]
        public void SimpleTransactionTest()
        {
            var expected = Transaction.Build(BlockchainRID)
                .AddOperation(Operation.Nop());
            var actual = Transaction.Decode(expected.Sign().SignedHash);
            Assert.Equal(expected.TransactionRID(), actual.TransactionRID);
        }

        [Fact]
        public void SignedTransactionTest()
        {
            var expected = Transaction.Build(BlockchainRID)
                .AddOperation(new Operation("test_op"))
                .AddSignatureProvider(Signer1);
            var actual = Transaction.Decode(expected.Sign().SignedHash);
            Assert.Equal(expected.TransactionRID(), actual.TransactionRID);
        }

        [Fact]
        public void SignedTransactionWithParameterTest()
        {
            var expected = Transaction.Build(BlockchainRID)
                .AddOperation(new Operation("test_op", "foo", "bar"))
                .AddSignatureProvider(Signer1);
            var actual = Transaction.Decode(expected.Sign().SignedHash);
            Assert.Equal(expected.TransactionRID(), actual.TransactionRID);
        }

        [Fact]
        public void SignedTransactionWithMultipleSignerTest()
        {
            var expected = Transaction.Build(BlockchainRID)
                .AddOperation(new Operation("test_op", "foo", "bar"))
                .AddSignatureProvider(Signer1)
                .AddSignatureProvider(Signer2);
            var actual = Transaction.Decode(expected.Sign().SignedHash);
            Assert.Equal(expected.TransactionRID(), actual.TransactionRID);
        }

        [Fact]
        public void SignedTransactionWithPreSignedTest()
        {
            var otherOp = Transaction.Build(BlockchainRID)
                .AddOperation(new Operation("test_op", "foo", "bar"))
                .AddSigner(Signer1.PubKey);
            var signature = otherOp.Sign(Signer2);

            var expected = Transaction.Build(BlockchainRID)
                .AddOperation(new Operation("test_op", "foo", "bar"))
                .AddSignatureProvider(Signer1);

            var actual = Transaction.Decode(expected.Sign(signature).SignedHash);
            Assert.Equal(expected.TransactionRID(), actual.TransactionRID);
        }
    }
}
