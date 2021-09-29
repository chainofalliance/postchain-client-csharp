using System;
using Xunit;

using Chromia.Postchain.Client.ASN1;

namespace Chromia.Postchain.Client.Tests.ASN1
{
    public class Asn1Test
    {
        [Fact]
        public void NullTest()
        {
            var writer = new AsnWriter();

            writer.WriteNull();

            var content = PostchainUtil.ByteArrayToString(writer.Encode()).ToUpper();

            var expected = "0500";
            Assert.Equal(expected, content);
        }

        [Fact]
        public void OctetStringTest()
        {
            var writer = new AsnWriter();

            writer.WriteOctetString(new byte[] { 0xaf, 0xfe });

            var content = PostchainUtil.ByteArrayToString(writer.Encode()).ToUpper();

            var expected = "0402AFFE";
            Assert.Equal(expected, content);
        }

        [Fact]
        public void EmptyOctetStringTest()
        {
            var writer = new AsnWriter();

            writer.WriteOctetString(new byte[] { });

            var content = PostchainUtil.ByteArrayToString(writer.Encode()).ToUpper();

            var expected = "0400";
            Assert.Equal(expected, content);
        }

        [Fact]
        public void BRIDOctetStringTest()
        {
            var writer = new AsnWriter();

            writer.WriteOctetString(PostchainUtil.HexStringToBuffer("E2BE5C617CE50AFD0882A753C6FDA9C4D925EEDAC50DB97E33F457826A856DE0"));

            var content = PostchainUtil.ByteArrayToString(writer.Encode()).ToUpper();

            var expected = "0420E2BE5C617CE50AFD0882A753C6FDA9C4D925EEDAC50DB97E33F457826A856DE0";
            Assert.Equal(expected, content);
        }

        [Fact]
        public void UTF8StringTest()
        {
            var writer = new AsnWriter();

            writer.WriteUTF8String("Hello World!");

            var content = PostchainUtil.ByteArrayToString(writer.Encode()).ToUpper();

            var expected = "0C0C48656C6C6F20576F726C6421";
            Assert.Equal(expected, content);
        }

        [Fact]
        public void EmptyUTF8StringTest()
        {
            var writer = new AsnWriter();

            writer.WriteUTF8String("");

            var content = PostchainUtil.ByteArrayToString(writer.Encode()).ToUpper();

            var expected = "0C00";
            Assert.Equal(expected, content);
        }

        [Fact]
        public void SepcialUTF8StringTest()
        {
            var writer = new AsnWriter();

            writer.WriteUTF8String("Swedish: Åå Ää Öö");
            writer.WriteUTF8String("Danish/Norway: Ææ Øø Åå");
            writer.WriteUTF8String("German/Finish: Ää Öö Üü");
            writer.WriteUTF8String("Greek lower: αβγδϵζηθικλμνξοπρστυϕχψω");
            writer.WriteUTF8String("Greek upper: ΑΒΓΔΕΖΗΘΙΚΛΜΝΞΟΠΡΣΤΥΦΧΨΩ");
            writer.WriteUTF8String("Russian: АаБбВвГгДдЕеЁёЖжЗзИиЙйКкЛлМмНнОоПпСсТтУуФфХхЦцЧчШшЩщЪъЫыЬьЭэЮюЯя");

            var content = PostchainUtil.ByteArrayToString(writer.Encode()).ToUpper();

            var expected = "0C17537765646973683A20C385C3A520C384C3A420C396C3B60C1D44616E6973682F4E6F727761793A20C386C3A620C398C3B820C385C3A50C1D4765726D616E2F46696E6973683A20C384C3A420C396C3B620C39CC3BC0C3D477265656B206C6F7765723A20CEB1CEB2CEB3CEB4CFB5CEB6CEB7CEB8CEB9CEBACEBBCEBCCEBDCEBECEBFCF80CF81CF83CF84CF85CF95CF87CF88CF890C3D477265656B2075707065723A20CE91CE92CE93CE94CE95CE96CE97CE98CE99CE9ACE9BCE9CCE9DCE9ECE9FCEA0CEA1CEA3CEA4CEA5CEA6CEA7CEA8CEA90C81895275737369616E3A20D090D0B0D091D0B1D092D0B2D093D0B3D094D0B4D095D0B5D081D191D096D0B6D097D0B7D098D0B8D099D0B9D09AD0BAD09BD0BBD09CD0BCD09DD0BDD09ED0BED09FD0BFD0A1D181D0A2D182D0A3D183D0A4D184D0A5D185D0A6D186D0A7D187D0A8D188D0A9D189D0AAD18AD0ABD18BD0ACD18CD0ADD18DD0AED18ED0AFD18F";
            Assert.Equal(expected, content);
        }

        [Fact]
        public void IntegerTest()
        {
            var writer = new AsnWriter();

            writer.WriteInteger(42424242);

            var content = PostchainUtil.ByteArrayToString(writer.Encode()).ToUpper();

            var expected = "0204028757B2";
            Assert.Equal(expected, content);
        }

        [Fact]
        public void NegativeIntegerTest()
        {
            var writer = new AsnWriter();

            writer.WriteInteger(-256);

            var content = PostchainUtil.ByteArrayToString(writer.Encode()).ToUpper();

            var expected = "0202FF00";
            Assert.Equal(expected, content);
        }

        [Fact]
        public void LimitIntegerTest()
        {
            var writer = new AsnWriter();

            writer.WriteInteger(Int64.MinValue);
            writer.WriteInteger(Int64.MaxValue);

            var content = PostchainUtil.ByteArrayToString(writer.Encode()).ToUpper();

            var expected = "0208800000000000000002087FFFFFFFFFFFFFFF";
            Assert.Equal(expected, content);
        }

        [Fact]
        public void SequenceTest()
        {
            var writer = new AsnWriter();

            writer.PushSequence();

            writer.PushSequence();
            writer.WriteOctetString(PostchainUtil.HexStringToBuffer("E2BE5C617CE50AFD0882A753C6FDA9C4D925EEDAC50DB97E33F457826A856DE0"));
            writer.PopSequence();

            writer.PushSequence();
            writer.PushSequence();
            writer.WriteUTF8String("test_op1");
            writer.WriteUTF8String("arg1");
            writer.WriteInteger(42);
            writer.PopSequence();
            writer.PushSequence();
            writer.WriteUTF8String("test_op2");
            writer.PopSequence();
            writer.PopSequence();

            writer.PopSequence();

            var content = PostchainUtil.ByteArrayToString(writer.Encode()).ToUpper();

            var expected = "304730220420E2BE5C617CE50AFD0882A753C6FDA9C4D925EEDAC50DB97E33F457826A856DE0302130130C08746573745F6F70310C046172673102012A300A0C08746573745F6F7032";
            Assert.Equal(expected, content);
        }
    }
}
