﻿using System;
using System.Collections.Generic;
using System.Linq;
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

        [Fact]
        public void SSOTransactionWithNullTest()
        {
            var encodedStr = "a582021b30820217a582020b30820207a1220420d5fc1055068a579e6e7b3c9b6b4e5d2f3d4c164cb6e88b2e5a7279d3db539e21a58201b6308201b2a58201ae308201aaa21d0c1b6674332e65766d2e6164645f617574685f64657363726970746f72a582018730820183a2420c4064326636303930613566666238333462363063636266663365353930303231373539393630643132313831666365356363643164626138373465313061363535a581a93081a6a2030c0153a5483046a2440c42303364666233656366623830616162333231383666353661316565643031613039613462643966616538383439613261303035636238623536353666316265373162a551304fa5073005a2030c0154a2440c42303364666233656366623830616162333231383666353661316565643031613039613462643966616538383439613261303035636238623536353666316265373162a0020500a5819030818da2420c4033636566313131623138313231306166306239663837373833613532366264653633636263393531373730366530643131633861333061666437303036613466a2420c4035363937313830613332343263326366623263343432653031666634363964393537633731623862333561313962643030393161373062323732383262376164a30302011ba5273025a123042103dfb3ecfb80aab32186f56a1eed01a09a4bd9fae8849a2a005cb8b5656f1be71ba5063004a1020400";
            var encoded = Buffer.From(encodedStr);

            var actual = Transaction.Decode(encoded);
            Assert.Equal(1, actual.Operations.Count);
            Assert.Contains("ft3.evm.add_auth_descriptor", actual.Operations.Select(o => o.Name));
        }

        [Fact]
        public void OtherSSOTransactionTest()
        {
            var encodedStr = "a582021b30820217a582020b30820207a12204207c07b9eaf0a544e047b361bfe43b64beac424d04821e089f975fb6dc1eec9beba58201b6308201b2a58201ae308201aaa21d0c1b6674332e65766d2e6164645f617574685f64657363726970746f72a582018730820183a2420c4062623236643132616333653535316332633665623236626163383836623965623764313830343338616236346632316263313931353762663866383839393831a581a93081a6a2030c0153a5483046a2440c42303238636163653462356439663263386339336364653866356363303035366631326466623563383036353165663335643035613163316265356131356562346639a551304fa5073005a2030c0154a2440c42303238636163653462356439663263386339336364653866356363303035366631326466623563383036353165663335643035613163316265356131356562346639a0020500a5819030818da2420c4030366366343433366138633434363834313365613338343063653237616134383961663431643664613236636632393931666163386364613731666230326531a2420c4032396235326462653735636132373634313932656237306330663039396563613364643236393035616138373465653761333737396338383232366663326331a30302011ba5273025a1230421028cace4b5d9f2c8c93cde8f5cc0056f12dfb5c80651ef35d05a1c1be5a15eb4f9a5063004a1020400";
            var encoded = Buffer.From(encodedStr);

            var actual = Transaction.Decode(encoded);
            Assert.Equal(1, actual.Operations.Count);
            Assert.Contains("ft3.evm.add_auth_descriptor", actual.Operations.Select(o => o.Name));
        }
    }
}
