using System.Security.Cryptography.Asn1;

namespace Chromia.PostchainClient.GTX.ASN1Messages
{
    public class DictPair
    {
        public string Name;
        public GTXValue Value;

        public DictPair(string name = "", GTXValue value = null)
        {
            this.Name = name;

            if (value == null)
            {
                this.Value = new GTXValue();
            }
            else
            {
                this.Value = value;
            }
        }

        public byte[] Encode()
        {
            var messageWriter = new AsnWriter(AsnEncodingRules.BER);
            
            messageWriter.PushSequence();
            messageWriter.WriteCharacterString(UniversalTagNumber.UTF8String, this.Name);
            messageWriter.WriteEncodedValue(Value.Encode());

            return messageWriter.Encode();
        }

        public static DictPair Decode(byte[] encodedMessage)
        {
            var dictPair = new AsnReader(encodedMessage, AsnEncodingRules.BER);
            var dictPairSequence = dictPair.ReadSequence();

            var newObject = new DictPair();
            newObject.Name = dictPairSequence.ReadCharacterString(UniversalTagNumber.UTF8String);
            newObject.Value = GTXValue.Decode(dictPair.PeekContentBytes().ToArray());

            return newObject;
        }
    }
}