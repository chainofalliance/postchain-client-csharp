using System.Security.Cryptography.Asn1;
using System.Linq;

namespace Chromia.PostchainClient.GTX.ASN1Messages
{    
    public enum GTXValueChoice
    {
        NotSet = -1,
        Null = 0,
        ByteArray = 1,
        String = 2,
        Integer = 3,
        Dict = 4,
        Array = 5
    }
    public class GTXValue
    {
        public GTXValueChoice Choice;
        public bool Null;
        public byte[] ByteArray;
        public string String;
        public int Integer;
        public DictPair[] Dict = null;
        public GTXValue[] Array = null;

        public GTXValue()
        {
            this.Choice = GTXValueChoice.NotSet;
        }

        public byte[] Encode()
        {
            var messageWriter = new AsnWriter(AsnEncodingRules.BER);

            byte[] choiceConstants = new byte[] {0x0, 0x0};
            switch (this.Choice)
            {
                case (GTXValueChoice.Null):
                {
                    messageWriter.WriteNull();
                    break;
                } 
                // The CHOICE in Asn1 is not implement in the used (experimental) library, yet.
                // Therefore we have to hack around a bit and create the bytes manually.
                // Since we can't seem to access the standard, we observed that the 2 octets are structured as follow:
                // |--0xa--| |--type--| |----length----|
                case (GTXValueChoice.ByteArray):
                {
                    choiceConstants = new byte[] {0xa1, (byte) (this.ByteArray.Length + 2)};
                    messageWriter.WriteOctetString(this.ByteArray);
                    break;
                }
                case (GTXValueChoice.String):
                {
                    choiceConstants = new byte[] {0xa2, (byte) (this.String.Length + 2)};
                    messageWriter.WriteCharacterString(UniversalTagNumber.UTF8String, this.String);
                    break;
                }
                case (GTXValueChoice.Integer):
                {
                    choiceConstants = new byte[] {0xa3, (byte) (Util.GetMaxAmountOfBytesForInteger(this.Integer) + 2)};
                    messageWriter.WriteInteger(this.Integer);
                    break;
                }
                case (GTXValueChoice.Dict):
                case (GTXValueChoice.Array):
                {
                    throw new System.Exception("Chromia.PostchainClient.GTX.Messages GTXValue.Encode() GTXValueChoice.Dict and GTXValueChoice.Array (of DictPair or GTXValue) not yet implemented.");
                }
                default:
                {
                    throw new System.Exception("Chromia.PostchainClient.GTX.Messages GTXValue.Encode() GTXValueChoice.Default case. Unknown choice " + this.Choice);
                }
            }

            return choiceConstants.Concat(messageWriter.Encode()).ToArray();
        }

        public static GTXValue Decode(byte[] encodedMessage)
        {
            var gtxValue = new AsnReader(encodedMessage, AsnEncodingRules.BER);

            var gtxValueData = new AsnReader(gtxValue.PeekContentBytes(), AsnEncodingRules.BER);

            var newObject = new GTXValue();
            switch (gtxValue.PeekTag().TagValue)
            {
                case ((int) Asn1TagValues.Null):
                {
                    newObject.Null = true;
                    newObject.Choice = GTXValueChoice.Null;
                    break;
                }                
                case ((int) Asn1TagValues.OctetString):
                {
                    newObject.ByteArray = gtxValue.ReadOctetString();
                    newObject.Choice = GTXValueChoice.ByteArray;
                    break;
                }
                case ((int) Asn1TagValues.UTF8String):
                {
                    newObject.String = gtxValue.ReadCharacterString(UniversalTagNumber.UTF8String);
                    newObject.Choice = GTXValueChoice.String;
                    break;
                }
                case ((int) Asn1TagValues.Integer):
                {
                    newObject.Integer = (int) gtxValue.ReadInteger();
                    newObject.Choice = GTXValueChoice.Integer;
                    break;
                }
                case ((int) Asn1TagValues.Sequence):
                {
                    throw new System.Exception("Chromia.PostchainClient.GTX.Messages GTXValue.Decode() Asn1TagValues.Sequence (of DictPair or GTXValue) not yet implemented.");
                }
                default:
                {
                    throw new System.Exception("Chromia.PostchainClient.GTX.Messages GTXValue.Decode() Asn1TagValues.Default case. Unknown tag " + gtxValue.PeekTag() + " (" + gtxValue.PeekTag().TagValue + ")");
                }
            }

            return newObject;
        }
    }
}