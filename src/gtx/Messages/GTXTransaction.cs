using System.Security.Cryptography.Asn1;
using System.Collections.Generic;

namespace Chromia.PostchainClient.GTX.ASN1Messages
{
    public class GTXTransaction
    {
        public byte[] BlockchainID;
        public List<GTXOperation> Operations;
        public List<byte[]> Signers;
        public List<byte[]> Signatures;

        public GTXTransaction(){
            this.BlockchainID = new byte[0];
            this.Operations = new List<GTXOperation>();
            this.Signers = new List<byte[]>();
            this.Signatures = new List<byte[]>();
        }

        public byte[] Encode()
        {
            var messageWriter = new AsnWriter(AsnEncodingRules.BER);
            messageWriter.PushSequence();

            messageWriter.WriteOctetString(this.BlockchainID);

            messageWriter.PushSequence();
            if (this.Operations.Count > 0)
            {
                foreach(var operation in this.Operations)
                {
                    messageWriter.WriteEncodedValue(operation.Encode());
                }
            }
            messageWriter.PopSequence();

            messageWriter.PushSequence();
            if (this.Signers.Count > 0)
            {
                foreach(var signer in this.Signers)
                {
                    messageWriter.WriteOctetString(signer);
                }
            }
            messageWriter.PopSequence();

            messageWriter.PushSequence();
            if (this.Signatures.Count > 0)
            {
                foreach(var signature in this.Signatures)
                {
                    messageWriter.WriteOctetString(signature);
                }
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
            newObject.Operations = Util.SequenceToList<GTXOperation>(operationSequence, GTXOperation.Decode);

            var signerSequence = gtxTransactionSequence.ReadSequence();
            newObject.Signers = Util.SequenceToList<byte[]>(signerSequence, null);

            var signatureSequence = gtxTransactionSequence.ReadSequence();
            newObject.Signatures = Util.SequenceToList<byte[]>(signatureSequence, null);

            return newObject;
        }
    }
}