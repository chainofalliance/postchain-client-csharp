using System.Security.Cryptography.Asn1;

namespace Chromia.PostchainClient.GTX.Messages
{
    public class GTXTransaction
    {
        public byte[] BlockchainID;
        public GTXOperation[] Operations;
        public byte[][] Signers;
        public byte[][] Signatures;

        public byte[] Encode()
        {
            var messageWriter = new AsnWriter(AsnEncodingRules.BER);
            messageWriter.PushSequence();

            messageWriter.WriteOctetString(this.BlockchainID);

            messageWriter.PushSequence();
            foreach(var operation in this.Operations)
            {
                messageWriter.WriteEncodedValue(operation.Encode());
            }
            messageWriter.PopSequence();

            messageWriter.PushSequence();
            foreach(var signer in this.Signers)
            {
                messageWriter.WriteOctetString(signer);
            }
            messageWriter.PopSequence();

            messageWriter.PushSequence();
            foreach(var signature in this.Signatures)
            {
                messageWriter.WriteOctetString(signature);
            }
            messageWriter.PopSequence();

            messageWriter.PopSequence();
            return messageWriter.Encode();
        }

        public static GTXTransaction Decode(byte[] encodedMessage)
        {
            var gtxTransaction = new AsnReader(encodedMessage, AsnEncodingRules.BER);
            var gtxTransactionSequence = gtxTransaction.ReadSequence();

            var newObject = new GTXTransaction();
            newObject.BlockchainID = gtxTransactionSequence.ReadOctetString();

            var operationSequence = gtxTransactionSequence.ReadSequence();
            newObject.Operations = Util.SequenceToArray<GTXOperation>(operationSequence, GTXOperation.Decode);

            var signerSequence = gtxTransactionSequence.ReadSequence();
            newObject.Signers = Util.SequenceToArray<byte[]>(signerSequence, null);

            var signatureSequence = gtxTransactionSequence.ReadSequence();
            newObject.Signatures = Util.SequenceToArray<byte[]>(signatureSequence, null);

            return newObject;
        }
    }
}