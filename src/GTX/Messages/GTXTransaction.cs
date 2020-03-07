using System.Collections.Generic;
using System.Linq;

namespace Chromia.Postchain.Client
{
    [System.Obsolete("Not used any more", true)]
    internal class GTXTransaction
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

        public override bool Equals(object obj)
        {
            if ((obj == null) || ! this.GetType().Equals(obj.GetType())) 
            {
                return false;
            }
            else { 
                GTXTransaction gtxTransaction = (GTXTransaction) obj;
                
                return ((this.BlockchainID == null || gtxTransaction.BlockchainID == null) ? this.BlockchainID == gtxTransaction.BlockchainID : Enumerable.SequenceEqual(this.BlockchainID, gtxTransaction.BlockchainID)) 
                    && ((this.Operations == null || gtxTransaction.Operations == null) ? this.Operations == gtxTransaction.Operations : Enumerable.SequenceEqual(this.Operations, gtxTransaction.Operations))
                    && ((this.Signers == null || gtxTransaction.Signers == null) ? this.Signers == gtxTransaction.Signers : Enumerable.SequenceEqual(this.Signers, gtxTransaction.Signers))
                    && ((this.Signatures == null || gtxTransaction.Signatures == null) ? this.Signatures == gtxTransaction.Signatures : Enumerable.SequenceEqual(this.Signatures, gtxTransaction.Signatures));
            }   
        }

        public override int GetHashCode()
        {
            return BlockchainID.GetHashCode();
        }

        public byte[] Encode()
        {
            var messageWriter = new AsnWriter();
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

        // public static GTXTransaction Decode(byte[] encodedMessage)
        // {
        //     var gtxTransaction = new AsnReader(encodedMessage, AsnEncodingRules.BER);
        //     var gtxTransactionSequence = gtxTransaction.ReadSequence();

        //     var newObject = new GTXTransaction();
        //     newObject.BlockchainID = gtxTransactionSequence.ReadOctetString();

        //     var operationSequence = gtxTransactionSequence.ReadSequence();
        //     newObject.Operations = ASN1Util.SequenceToList<GTXOperation>(operationSequence, GTXOperation.Decode);

        //     var signerSequence = gtxTransactionSequence.ReadSequence();
        //     newObject.Signers = ASN1Util.SequenceToList<byte[]>(signerSequence, null);

        //     var signatureSequence = gtxTransactionSequence.ReadSequence();
        //     newObject.Signatures = ASN1Util.SequenceToList<byte[]>(signatureSequence, null);

        //     return newObject;
        // }
    }
}