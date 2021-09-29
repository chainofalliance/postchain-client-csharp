using System;
using Xunit;
using Newtonsoft.Json;

namespace Chromia.Postchain.Client.Tests
{
    public class OperationTest : PrintableTest
    {
        private bool PRINT_SSO_TEST = false;

        [Fact]
        public void EmptyOperationTest()
        {
            var keys = PostchainUtil.MakeKeyPair();
            var gtx = new Gtx("abcdef1234567890abcdef1234567890");

            gtx.AddOperationToGtx("test", new object[] { });
            gtx.AddSignerToGtx(keys["pubKey"]);
            gtx.Sign(keys["privKey"], keys["pubKey"]);

            var encode = gtx.Encode();
            if (PRINT_CONTENT)
            {
                Console.WriteLine("EmptyOperationTest: " + PostchainUtil.ByteArrayToString(encode).ToUpper());
            }
            var decoded = Gtx.Decode(encode);

            Assert.Equal(gtx.BlockchainID, decoded.BlockchainID);
            Assert.Equal(gtx.Signatures, decoded.Signatures);
            Assert.Equal(gtx.Signers, decoded.Signers);

            var gtxOpJson = JsonConvert.SerializeObject(gtx.Operations);
            var decOpJson = JsonConvert.SerializeObject(decoded.Operations);
            Assert.Equal(gtxOpJson, decOpJson);
        }

        [Fact]
        public void SimpleOperationTest()
        {
            var keys = PostchainUtil.MakeKeyPair();
            var gtx = new Gtx("abcdef1234567890abcdef1234567890");

            gtx.AddOperationToGtx("test", new object[] { "teststring" });
            gtx.AddSignerToGtx(keys["pubKey"]);
            gtx.Sign(keys["privKey"], keys["pubKey"]);

            var encode = gtx.Encode();
            if (PRINT_CONTENT)
            {
                Console.WriteLine("SimpleOperationTest: " + PostchainUtil.ByteArrayToString(encode).ToUpper());
            }
            var decoded = Gtx.Decode(encode);

            Assert.Equal(gtx.BlockchainID, decoded.BlockchainID);
            Assert.Equal(gtx.Signatures, decoded.Signatures);
            Assert.Equal(gtx.Signers, decoded.Signers);

            var gtxOpJson = JsonConvert.SerializeObject(gtx.Operations);
            var decOpJson = JsonConvert.SerializeObject(decoded.Operations);
            Assert.Equal(gtxOpJson, decOpJson);
        }

        [Fact]
        public void MultiSigOperationTest()
        {
            var keys1 = PostchainUtil.MakeKeyPair();
            var keys2 = PostchainUtil.MakeKeyPair();
            var gtx = new Gtx("abcdef1234567890abcdef1234567890");

            gtx.AddOperationToGtx("test", new object[] { "teststring" });

            gtx.AddSignerToGtx(keys1["pubKey"]);
            gtx.AddSignerToGtx(keys2["pubKey"]);

            gtx.Sign(keys1["privKey"], keys1["pubKey"]);
            gtx.Sign(keys2["privKey"], keys2["pubKey"]);

            var encode = gtx.Encode();
            if (PRINT_CONTENT)
            {
                Console.WriteLine("MultiSigOperationTest: " + PostchainUtil.ByteArrayToString(encode).ToUpper());
            }
            var decoded = Gtx.Decode(encode);

            Assert.Equal(gtx.BlockchainID, decoded.BlockchainID);
            Assert.Equal(gtx.Signatures, decoded.Signatures);
            Assert.Equal(gtx.Signers, decoded.Signers);

            var gtxOpJson = JsonConvert.SerializeObject(gtx.Operations);
            var decOpJson = JsonConvert.SerializeObject(decoded.Operations);
            Assert.Equal(gtxOpJson, decOpJson);
        }

        [Fact]
        public void FullOperationTest()
        {
            var keys = PostchainUtil.MakeKeyPair();
            var gtx = new Gtx("abcdef1234567890abcdef1234567890");

            gtx.AddOperationToGtx("test", new object[] { "teststring", 123, new byte[] { 0xaf, 0xfe } });
            gtx.AddSignerToGtx(keys["pubKey"]);
            gtx.Sign(keys["privKey"], keys["pubKey"]);

            var encode = gtx.Encode();
            if (PRINT_CONTENT)
            {
                Console.WriteLine("FullOperationTest: " + PostchainUtil.ByteArrayToString(encode).ToUpper());
            }
            var decoded = Gtx.Decode(encode);

            Assert.Equal(gtx.BlockchainID, decoded.BlockchainID);
            Assert.Equal(gtx.Signatures, decoded.Signatures);
            Assert.Equal(gtx.Signers, decoded.Signers);

            var gtxOpJson = JsonConvert.SerializeObject(gtx.Operations);
            var decOpJson = JsonConvert.SerializeObject(decoded.Operations);
            Assert.Equal(gtxOpJson, decOpJson);
        }

        [Fact]
        public void LengthEdgeCaseOperationTest()
        {
            var keys = PostchainUtil.MakeKeyPair();
            var gtx = new Gtx("abcdef1234567890abcdef1234567890");

            gtx.AddOperationToGtx("test", new object[] { "teststring", 123, new byte[] { 0xaf, 0xfe }, new string[] { "hello", "world" } });
            gtx.AddSignerToGtx(keys["pubKey"]);
            gtx.Sign(keys["privKey"], keys["pubKey"]);

            var encode = gtx.Encode();
            if (PRINT_CONTENT)
            {
                Console.WriteLine("LengthEdgeCaseOperationTest: " + PostchainUtil.ByteArrayToString(encode).ToUpper());
            }
            var decoded = Gtx.Decode(encode);

            Assert.Equal(gtx.BlockchainID, decoded.BlockchainID);
            Assert.Equal(gtx.Signatures, decoded.Signatures);
            Assert.Equal(gtx.Signers, decoded.Signers);

            var gtxOpJson = JsonConvert.SerializeObject(gtx.Operations);
            var decOpJson = JsonConvert.SerializeObject(decoded.Operations);
            Assert.Equal(gtxOpJson, decOpJson);
        }

        [Fact]
        public void SSORegisterOperationTest()
        {
            var keys = PostchainUtil.MakeKeyPair();
            var gtx = new Gtx("abcdef1234567890abcdef1234567890");

            gtx.AddOperationToGtx("ft3.dev_register_account", new object[]{
                PostchainUtil.HexStringToBuffer("abcdef1234567890abcdef1234567890"),
                keys["pubKey"],
                null,
                PostchainUtil.HexStringToBuffer("abcdef1234567890abcdef1234567890")
            });
            gtx.AddSignerToGtx(keys["pubKey"]);
            gtx.Sign(keys["privKey"], keys["pubKey"]);

            var encode = gtx.Encode();
            if (PRINT_CONTENT)
            {
                Console.WriteLine("SSORegisterOperationTest: " + PostchainUtil.ByteArrayToString(encode).ToUpper());
            }
            var decoded = Gtx.Decode(encode);

            Assert.Equal(gtx.BlockchainID, decoded.BlockchainID);
            Assert.Equal(gtx.Signatures, decoded.Signatures);
            Assert.Equal(gtx.Signers, decoded.Signers);

            var gtxOpJson = JsonConvert.SerializeObject(gtx.Operations);
            var decOpJson = JsonConvert.SerializeObject(decoded.Operations);
            Assert.Equal(gtxOpJson, decOpJson);
        }

        [Fact]
        public void SSOAddAuthOperationTest()
        {
            var keys = PostchainUtil.MakeKeyPair();
            var gtx = new Gtx("abcdef1234567890abcdef1234567890");

            gtx.AddOperationToGtx("ft3.add_auth_descriptor", new object[]{
                PostchainUtil.HexStringToBuffer("abcdef1234567890abcdef1234567890"),
                PostchainUtil.HexStringToBuffer("abcdef1234567890abcdef1234567890"),
                new object[]{
                    PostchainUtil.HexStringToBuffer("abcdef1234567890abcdef1234567890"),
                    keys["pubKey"],
                    null,
                    PostchainUtil.HexStringToBuffer("abcdef1234567890abcdef1234567890")
                }
            });
            gtx.AddSignerToGtx(keys["pubKey"]);
            gtx.Sign(keys["privKey"], keys["pubKey"]);

            var encode = gtx.Encode();
            if (PRINT_CONTENT)
            {
                Console.WriteLine("SSOAddAuthOperationTest: " + PostchainUtil.ByteArrayToString(encode).ToUpper());
            }
            var decoded = Gtx.Decode(encode);

            Assert.Equal(gtx.BlockchainID, decoded.BlockchainID);
            Assert.Equal(gtx.Signatures, decoded.Signatures);
            Assert.Equal(gtx.Signers, decoded.Signers);

            var gtxOpJson = JsonConvert.SerializeObject(gtx.Operations);
            var decOpJson = JsonConvert.SerializeObject(decoded.Operations);
            Assert.Equal(gtxOpJson, decOpJson);
        }

        [Fact]
        public void SSOCodeTest()
        {
            var encode = PostchainUtil.HexStringToBuffer("A582030A30820306A58202B6308202B2A12204201A3A5B4C919798B52292094185B37E71898CC245FA9F0AC51A33B473150FE889A582023C30820238A581D63081D3A21A0C186674332E6465765F72656769737465725F6163636F756E74A581B43081B1A581AE3081ABA2030C0153A5483046A2440C42303364663064333232333864323130303031663164656637373464393031303132343565343235633839353366623435373061633630313961303665373163633732A5563054A50C300AA2030C0141A2030C0154A2440C42303364663064333232333864323130303031663164656637373464393031303132343565343235633839353366623435373061633630313961303665373163633732A0020500A582015B30820157A2190C176674332E6164645F617574685F64657363726970746F72A582013830820134A2420C4036376331646431316332393630613836663261663933633939626538326333353530303734353839636234366531383564346538363030316439376464646466A2420C4036376331646431316332393630613836663261663933633939626538326333353530303734353839636234366531383564346538363030316439376464646466A581A93081A6A2030C0153A5483046A2440C42303332666462616430643537383439343436353633346661646339633534333066333865663566623231333634353731643637313464363036393336633530643434A551304FA5073005A2030C0154A2440C42303332666462616430643537383439343436353633346661646339633534333066333865663566623231333634353731643637313464363036393336633530643434A0020500A54C304AA123042103DF0D32238D210001F1DEF774D90101245E425C8953FB4570AC6019A06E71CC72A1230421032FDBAD0D578494465634FADC9C5430F38EF5FB21364571D6714D606936C50D44A54A3048A14204406F54F8D0B38A90414C1DD4C84D2D2B1553822F8F6D026058BD601221367958E379416A6E771944DA2B6DCFEE05A0A4FC875A90BA04F0C59637C7BEC9A646B890A1020400");
            var decoded = Gtx.Decode(encode);

            if (PRINT_SSO_TEST)
            {
                Console.Write("BRID: ");
                Console.WriteLine(decoded.BlockchainID);

                Console.WriteLine("\nSignatures: ");
                foreach (var sig in decoded.Signatures)
                {
                    Console.WriteLine(PostchainUtil.ByteArrayToString(sig));
                }


                Console.WriteLine("Signers: ");
                foreach (var signer in decoded.Signers)
                {
                    Console.WriteLine(PostchainUtil.ByteArrayToString(signer));
                }

                Console.WriteLine("\nOperations: ");
                foreach (var op in decoded.Operations)
                {
                    Console.WriteLine(op);
                }
            }
        }
    }
}
