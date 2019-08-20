using System.Security.Cryptography.Asn1;

namespace Chromia.PostchainClient.GTX.Messages
{
    public class GTXOperation
    {
        public string OpName;
        public GTXValue[] Args;

        public byte[] Encode()
        {
            var messageWriter = new AsnWriter(AsnEncodingRules.BER);
            messageWriter.PushSequence();

            messageWriter.WriteCharacterString(UniversalTagNumber.UTF8String, this.OpName);

            messageWriter.PushSequence();
            foreach(var arg in this.Args)
            {
                messageWriter.WriteEncodedValue(arg.Encode());
            }
            messageWriter.PopSequence();

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
            newObject.Args = Util.SequenceToArray<GTXValue>(valueSequence, GTXValue.Decode);

            return newObject;
        }
    }
}