﻿using System;
using System.Linq;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace Chromia.Tests.Encoding
{
    public class TransactionTest : PrintableTest
    {
        private const int HASH_VERSION_2 = 2;
        private SignatureProvider Signer1 => StaticSignatureProvider.Signer1;
        private SignatureProvider Signer2 => StaticSignatureProvider.Signer2;
        private SignatureProvider Signer3 => StaticSignatureProvider.Signer3;
        private SignatureProvider Signer4 => StaticSignatureProvider.Signer4;

        public TransactionTest(ITestOutputHelper output) : base(output)
        {
        }

        private static readonly Buffer BlockchainRID = Buffer.From("7848422629D30011403E100FCB910A9A364677A9E84D05A66B6320D0E261EF71");


        [Fact]
        public void EmptyTransactionTest()
        {
            var expected = Transaction.Build(BlockchainRID, HASH_VERSION_2);
            var actual = Transaction.Decode(expected.Sign().GtvBody, HASH_VERSION_2);
            Assert.Equal(expected.TransactionRID(), actual.TransactionRID);
        }

        [Fact]
        public void SimpleTransactionTest()
        {
            var expected = Transaction.Build(BlockchainRID, HASH_VERSION_2)
                .AddOperation(Operation.Nop());
            var actual = Transaction.Decode(expected.Sign().GtvBody, HASH_VERSION_2);
            Assert.Equal(expected.TransactionRID(), actual.TransactionRID);
        }

        [Fact]
        public void SignedTransactionTest()
        {
            var expected = Transaction.Build(BlockchainRID, HASH_VERSION_2)
                .AddOperation(new Operation("test_op"))
                .AddSignatureProvider(Signer1);
            var actual = Transaction.Decode(expected.Sign().GtvBody, HASH_VERSION_2);
            Assert.Equal(expected.TransactionRID(), actual.TransactionRID);
        }

        [Fact]
        public void SignedTransactionWithParameterTest()
        {
            var expected = Transaction.Build(BlockchainRID, HASH_VERSION_2)
                .AddOperation(new Operation("test_op", "foo", "bar"))
                .AddSignatureProvider(Signer1);
            var actual = Transaction.Decode(expected.Sign().GtvBody, HASH_VERSION_2);
            Assert.Equal(expected.TransactionRID(), actual.TransactionRID);
        }

        [Fact]
        public void SignedTransactionWithMultipleSignerTest()
        {
            var expected = Transaction.Build(BlockchainRID, HASH_VERSION_2)
                .AddOperation(new Operation("test_op", "foo", "bar"))
                .AddSignatureProvider(Signer1)
                .AddSignatureProvider(Signer2);
            var actual = Transaction.Decode(expected.Sign().GtvBody, HASH_VERSION_2);
            Assert.Equal(expected.TransactionRID(), actual.TransactionRID);
        }

        [Fact]
        public void SignedTransactionWithPreSignedTest()
        {
            var otherOp = Transaction.Build(BlockchainRID, HASH_VERSION_2)
                .AddOperation(new Operation("test_op", "foo", "bar"))
                .AddSigner(Signer1.PubKey);
            var signature = otherOp.Sign(Signer2);

            var expected = Transaction.Build(BlockchainRID, HASH_VERSION_2)
                .AddOperation(new Operation("test_op", "foo", "bar"))
                .AddSignatureProvider(Signer1);

            var actual = Transaction.Decode(expected.Sign(signature).GtvBody, HASH_VERSION_2);
            Assert.Equal(expected.TransactionRID(), actual.TransactionRID);
        }

        [Fact]
        public void SignedTransactionWithPreSigned2Test()
        {
            var otherTx = Transaction.Build(BlockchainRID, HASH_VERSION_2)
                .AddOperation(new Operation("test_op", "foo", "bar"))
                .AddSigner(Signer1.PubKey)
                .AddSigner(Signer3.PubKey)
                .AddSignatureProvider(Signer2);
            var signedTx = otherTx.Sign();
            var newSignedTx = signedTx.Sign(Signer1, HASH_VERSION_2);

            Assert.Equal(3, signedTx.Signers.Count);
            Assert.Equal(1, signedTx.Signatures.Count);
            Assert.Equal(3, newSignedTx.Signers.Count);
            Assert.Equal(2, newSignedTx.Signatures.Count);

            var decodedTx = Transaction.Decode(newSignedTx.GtvBody, HASH_VERSION_2);
            newSignedTx = decodedTx.Sign(Signer3, HASH_VERSION_2);

            Assert.Equal(3, decodedTx.Signers.Count);
            Assert.Equal(2, decodedTx.Signatures.Count);
            Assert.Equal(3, newSignedTx.Signers.Count);
            Assert.Equal(3, newSignedTx.Signatures.Count);

            Assert.Throws<ArgumentException>(() => { newSignedTx.Sign(Signer4, HASH_VERSION_2); });
        }

        [Fact]
        public void SSOTransactionWithNullTest()
        {
            var encodedStr = "a582021b30820217a582020b30820207a1220420d5fc1055068a579e6e7b3c9b6b4e5d2f3d4c164cb6e88b2e5a7279d3db539e21a58201b6308201b2a58201ae308201aaa21d0c1b6674332e65766d2e6164645f617574685f64657363726970746f72a582018730820183a2420c4064326636303930613566666238333462363063636266663365353930303231373539393630643132313831666365356363643164626138373465313061363535a581a93081a6a2030c0153a5483046a2440c42303364666233656366623830616162333231383666353661316565643031613039613462643966616538383439613261303035636238623536353666316265373162a551304fa5073005a2030c0154a2440c42303364666233656366623830616162333231383666353661316565643031613039613462643966616538383439613261303035636238623536353666316265373162a0020500a5819030818da2420c4033636566313131623138313231306166306239663837373833613532366264653633636263393531373730366530643131633861333061666437303036613466a2420c4035363937313830613332343263326366623263343432653031666634363964393537633731623862333561313962643030393161373062323732383262376164a30302011ba5273025a123042103dfb3ecfb80aab32186f56a1eed01a09a4bd9fae8849a2a005cb8b5656f1be71ba5063004a1020400";
            var encoded = Buffer.From(encodedStr);

            var actual = Transaction.Decode(encoded, hashVersion: 1);
            Assert.Equal(1, actual.Operations.Count);
            Assert.Contains("ft3.evm.add_auth_descriptor", actual.Operations.Select(o => o.Name));
        }

        [Fact]
        public void OtherSSOTransactionTest()
        {
            var encodedStr = "a582021b30820217a582020b30820207a12204207c07b9eaf0a544e047b361bfe43b64beac424d04821e089f975fb6dc1eec9beba58201b6308201b2a58201ae308201aaa21d0c1b6674332e65766d2e6164645f617574685f64657363726970746f72a582018730820183a2420c4062623236643132616333653535316332633665623236626163383836623965623764313830343338616236346632316263313931353762663866383839393831a581a93081a6a2030c0153a5483046a2440c42303238636163653462356439663263386339336364653866356363303035366631326466623563383036353165663335643035613163316265356131356562346639a551304fa5073005a2030c0154a2440c42303238636163653462356439663263386339336364653866356363303035366631326466623563383036353165663335643035613163316265356131356562346639a0020500a5819030818da2420c4030366366343433366138633434363834313365613338343063653237616134383961663431643664613236636632393931666163386364613731666230326531a2420c4032396235326462653735636132373634313932656237306330663039396563613364643236393035616138373465653761333737396338383232366663326331a30302011ba5273025a1230421028cace4b5d9f2c8c93cde8f5cc0056f12dfb5c80651ef35d05a1c1be5a15eb4f9a5063004a1020400";
            var encoded = Buffer.From(encodedStr);

            var actual = Transaction.Decode(encoded, hashVersion: 1);
            Assert.Equal(1, actual.Operations.Count);
            Assert.Contains("ft3.evm.add_auth_descriptor", actual.Operations.Select(o => o.Name));
        }

        [Fact]
        public void MasterAuthTransactionTest()
        {
            var encodedStr = "a582016d30820169a582011d30820119a122042072b3258876e8249dfbc1fa347b47b497426c6e7748883f4eb9d00afd659e2345a581c93081c6a55d305ba20d0c0b6674342e66745f61757468a54a3048a122042099fd2eb50911ae53372b79319ecd004c2d0c682faff90635619ce86762ecf23da122042099fd2eb50911ae53372b79319ecd004c2d0c682faff90635619ce86762ecf23da5653063a2190c176674342e6164645f617574685f64657363726970746f72a5463044a5423040a303020100a5353033a50c300aa2030c0141a2030c0154a1230421036c741014f8ed3945e7473befdb40aa4ded1cd010f9978a4285622274ca04ff5aa0020500a5273025a1230421036c741014f8ed3945e7473befdb40aa4ded1cd010f9978a4285622274ca04ff5aa5463044a1420440fe63bd1d8ca95a13bbc123875738191fef8d01f77d1e382cba2a3ae0c045a15e6d224a7b575ef8da22900b68c949a761c3c2da0ad8505920f1e7fe488b42d6b7";
            var encoded = Buffer.From(encodedStr);

            var actual = Transaction.Decode(encoded, hashVersion: 1);
            Console.WriteLine(actual.TransactionRID);
            Console.WriteLine(actual.BlockchainRID);
            Console.WriteLine(actual.Operations.First());
            Console.WriteLine(actual.Operations.Skip(1).First());
            Console.WriteLine(actual.Signers.First());
        }

        [Fact]
        public void CoaLoginTest()
        {
            var encodedStr = "A58201C4308201C0A582017430820170A122042003A475768FDFC713C75BF143BE75A0189D1B8678C1A321DDC2686D0BD28BA0C6A582011F3082011BA581B63081B3A20E0C0C6674342E65766D5F61757468A581A030819DA12204200DCC1E65BD4B1D6EF678F29F73EF1C20AA864662E44E97A497291BD5B53F34D4A12204202444D3D99356095D20C68FB659210F5013AD9708C62CEB9FB5363F2E9CA0AD36A5533051A54F304DA12204204A16B107CD144EF433D01578ACBB41DED0CF68782D925B177D750E94819C4AFDA122042041D09F0630246F30DC8936FB28108EE66D07F4918418ECC4C52A39DEB92E320DA30302011CA560305EA2190C176674342E6164645F617574685F64657363726970746F72A541303FA53D303BA303020100A530302EA5073005A2030C0154A1230421028B49D8A1B0749CD7D9F9BAA24A197FB5709998D755A7B05CECE747F972647905A0020500A5273025A1230421028B49D8A1B0749CD7D9F9BAA24A197FB5709998D755A7B05CECE747F972647905A5463044A1420440860FD5FE5B81F79B6352771E70155197F2BAB1B2BF76F235585E9524E127315B5D9A1285D83155B91BD47C502BBB84B1220ADA597AA1E6EEFFBC5623221DDBD7";
            var encoded = Buffer.From(encodedStr);

            var actual = Transaction.Decode(encoded, hashVersion: 1);
            Console.WriteLine(actual.TransactionRID);
            Console.WriteLine(actual.BlockchainRID);
            Console.WriteLine(actual.Operations.First());
            Console.WriteLine(actual.Operations.Skip(1).First());
        }
    }
}
