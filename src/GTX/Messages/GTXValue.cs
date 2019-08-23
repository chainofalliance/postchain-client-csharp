using System.Security.Cryptography.Asn1;
using System.Linq;
using System.Collections.Generic;
using System;

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
        public byte[] ByteArray;
        public string String;
        public int Integer;
        public List<DictPair> Dict = null;
        public List<GTXValue> Array = null;

        public GTXValue()
        {
            this.Choice = GTXValueChoice.NotSet;
        }

        public override bool Equals(object obj)
        {
            if ((obj == null) || ! this.GetType().Equals(obj.GetType())) 
            {
                return false;
            }
            else { 
                GTXValue gtxValue = (GTXValue) obj;
                
                return this.Choice.Equals(gtxValue.Choice) 
                    && ((this.ByteArray == null || gtxValue.ByteArray == null) ? this.ByteArray == gtxValue.ByteArray : Enumerable.SequenceEqual(this.ByteArray, gtxValue.ByteArray))
                    && this.Integer.Equals(gtxValue.Integer)
                    && ((this.Dict == null || gtxValue.Dict == null) ? this.Dict == gtxValue.Dict : Enumerable.SequenceEqual(this.Dict, gtxValue.Dict))
                    && ((this.Array == null || gtxValue.Array == null) ? this.Array == gtxValue.Array : Enumerable.SequenceEqual(this.Array, gtxValue.Array));
            }   
        }

        public override int GetHashCode()
        {
            return Choice.GetHashCode()
                + ByteArray.GetHashCode()
                + Integer.GetHashCode()
                + Dict.GetHashCode()
                + Array.GetHashCode();
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
                    choiceConstants = new byte[] {0xa1, GetValueSize(this)};
                    messageWriter.WriteOctetString(this.ByteArray);
                    break;
                }
                case (GTXValueChoice.String):
                {
                    choiceConstants = new byte[] {0xa2, GetValueSize(this)};
                    messageWriter.WriteCharacterString(UniversalTagNumber.UTF8String, this.String);
                    break;
                }
                case (GTXValueChoice.Integer):
                {
                    this.Integer = Math.Abs(this.Integer);
                    choiceConstants = new byte[] {0xa3, GetValueSize(this)};
                    messageWriter.WriteInteger(this.Integer);
                    break;
                }
                case (GTXValueChoice.Array):
                {

                    choiceConstants = new byte[] {0xa5, (byte) (GetValueSize(this))};

                    messageWriter.PushSequence();
                    foreach (var gtxValue in this.Array)
                    {
                        messageWriter.WriteEncodedValue(gtxValue.Encode());
                    }
                    messageWriter.PopSequence();

                    break;
                }
                case (GTXValueChoice.Dict):
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

        private static byte GetValueSize(GTXValue gtxValue)
        {
            switch (gtxValue.Choice)
            {
                case (GTXValueChoice.ByteArray):
                {
                    return (byte) (gtxValue.ByteArray.Length + 2);
                }
                case (GTXValueChoice.String):
                {
                    return (byte) (gtxValue.String.Length + 2);
                }
                case (GTXValueChoice.Integer):
                {                    
                    return (byte) (ASN1Util.GetMaxAmountOfBytesForInteger(gtxValue.Integer) + 2);
                }
                case (GTXValueChoice.Array):
                {
                    byte choiceSize = (byte) (2 + (gtxValue.Array.Count * 2));

                    foreach (var val in gtxValue.Array)
                    {
                        choiceSize += GetValueSize(val);
                    }

                    return choiceSize;
                }
                case (GTXValueChoice.Dict):
                {
                    throw new System.Exception("Chromia.PostchainClient.GTX.Messages GTXValue.Encode() GTXValueChoice.Dict and GTXValueChoice.Array (of DictPair or GTXValue) not yet implemented.");
                }
                default:
                {
                    throw new System.Exception("Chromia.PostchainClient.GTX.Messages GTXValue.GetValueSize() GTXValueChoice.Default case. Unknown choice " + gtxValue.Choice);
                }
            }
        }

        public static GTXValue Decode(byte[] encodedMessage)
        {
            //Since the ASN1 library can't en-/decode CHOICEs, we just skip over the specific octets
            var gtxValue = new AsnReader(encodedMessage.Skip(2).ToArray(), AsnEncodingRules.BER);

            var gtxValueData = new AsnReader(gtxValue.PeekContentBytes(), AsnEncodingRules.BER);

            var newObject = new GTXValue();
            switch (gtxValue.PeekTag().TagValue)
            {
                case ((int) Asn1TagValues.Null):
                {
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