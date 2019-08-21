using System.Security.Cryptography.Asn1;
using System.Collections.Generic;

namespace Chromia.PostchainClient.GTX.ASN1Messages
{
    public class GTXOperation
    {
        public string OpName;
        public List<GTXValue> Args;

        public GTXOperation(){
            this.OpName = "";
            this.Args = new List<GTXValue>();
        }

        public byte[] Encode()
        {
            var messageWriter = new AsnWriter(AsnEncodingRules.BER);
            messageWriter.PushSequence();

            messageWriter.WriteCharacterString(UniversalTagNumber.UTF8String, this.OpName);

            if (this.Args.Count > 0)
            {
                messageWriter.PushSequence();
                foreach(var arg in this.Args)
                {
                    messageWriter.WriteEncodedValue(arg.Encode());
                }
                messageWriter.PopSequence();
            }

            messageWriter.PopSequence();
            return messageWriter.Encode();
        }

        public static GTXOperation Decode(byte[] encodedMessage)
        {
            var gtxOperation = new AsnReader(encodedMessage, AsnEncodingRules.BER);
            var gtxOperationSequence = gtxOperation.ReadSequence();

            var newObject = new GTXOperation();
            newObject.OpName = gtxOperationSequence.ReadCharacterString(UniversalTagNumber.UTF8String);

            var valueSequence = gtxOperationSequence.ReadSequence();
            newObject.Args = ASN1Util.SequenceToList<GTXValue>(valueSequence, GTXValue.Decode);

            return newObject;
        }
    }
}