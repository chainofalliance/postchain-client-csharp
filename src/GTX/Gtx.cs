using System.Linq;
using System;
using Chromia.PostchainClient.GTX.ASN1Messages;

namespace Chromia.PostchainClient.GTX
{

    public class Gtx
    {
        private GTXTransaction Transaction;

        public Gtx(string blockchainRID = "")
        {
            this.Transaction = new GTXTransaction();
            this.Transaction.BlockchainID = ASN1Util.StringToByteArray(blockchainRID);
        }

        /**
        * Adds a function call to a GTX. Creates a new GTX if none specified.
        * This function will throw Error if gtx is already signed
        * @param opName the name of the function to call
        * @param args the array of arguments of the function call. If no args, this must be an empty array
        * @param gtx the function call will be added to this gtx
        * @returns the gtx
        * @throws if gtx is null or if gtx is already signed
        */
        public Gtx AddOperationToGtx(string opName, dynamic[] args)
        {
           if(this.Transaction.Signatures.Count != 0)
            {
                throw new Exception("Cannot add function calls to an already signed gtx");
            }

            var newOperation = new GTXOperation(opName);

            foreach (var arg in args)
            {
                newOperation.Args.Add(ArgToGTXValue(arg));
            }

            this.Transaction.Operations.Add(newOperation);
   
            return this;
        }

        private GTXValue ArgToGTXValue(dynamic arg)
        {
            var gtxValue = new GTXValue();
            
            if (arg == null)
            {
                gtxValue.Choice = GTXValueChoice.Null;
            }
            else if (arg is int)
            {
                gtxValue.Choice = GTXValueChoice.Integer;
                gtxValue.Integer = (int) arg;
            }
            else if (arg is byte[])
            {
                gtxValue.Choice = GTXValueChoice.ByteArray;
                gtxValue.ByteArray = (byte[]) arg;
            }
            else if (arg is string)
            {
                gtxValue.Choice = GTXValueChoice.String;
                gtxValue.String = (string) arg;
            }
            else if (arg is dynamic[])
            {
                gtxValue.Choice = GTXValueChoice.Array;

                foreach (dynamic subArg in (dynamic[]) arg)
                {
                    gtxValue.Array.Add(ArgToGTXValue(subArg));
                }
            }
            else if (arg is Tuple<string, dynamic>[])
            {
                gtxValue.Choice = GTXValueChoice.Dict;

                foreach (Tuple<string, dynamic> subArg in arg)
                {
                    gtxValue.Dict.Add(new DictPair(subArg.Item1, ArgToGTXValue(subArg.Item2)));
                }
            }


            return gtxValue;
        }

        public void AddSignerToGtx(byte[] signer)
        {
            if(this.Transaction.Signers.Count != 0)
            {
                throw new Exception("Cannot add signers to an already signed gtx");
            }

            this.Transaction.Signers.Add(signer);
        }

        public void Sign(byte[] privKey, byte[] pubKey)
        {
            byte[] bufferToSign = this.GetBufferToSign();
            var signature = Util.Sign(bufferToSign, privKey);
            Console.WriteLine("PubKey: " + BitConverter.ToString(pubKey));
            Console.WriteLine("Signature: " + BitConverter.ToString(signature));
            this.AddSignature(pubKey, signature);
        }

        /**
        * Serializes the gtx for signing
        * @param gtx the gtx to serialize
        */
        public byte[] GetBufferToSign()
        {
            var oldSignatures = this.Transaction.Signatures;
            this.Transaction.Signatures.Clear();

            var encodedBuffer = this.Transaction.Encode();
            this.Transaction.Signatures = oldSignatures;

            return encodedBuffer;
        }

        public void AddSignature(byte[] pubKeyBuffer, byte[] signatureBuffer)
        {   
            foreach(var signer in this.Transaction.Signers)
            {
                this.Transaction.Signatures.Add(null);
            }

            if (this.Transaction.Signers.Count != this.Transaction.Signatures.Count) {
                throw new Exception("Mismatching signers and signatures");
            } 
            var signerIndex = this.Transaction.Signers.FindIndex(signer => signer.SequenceEqual(pubKeyBuffer));

            if (signerIndex == -1) {
                throw new Exception("No such signer, remember to call addSignerToGtx() before adding a signature");
            }

            this.Transaction.Signatures[signerIndex] = signatureBuffer;
        }

        public string Serialize()
        {
           return ASN1Util.ByteArrayToString(this.Transaction.Encode());
        }

        public static Gtx Deserialize(byte[] gtxBytes)
        {
            var newGTXObject = new Gtx();
            newGTXObject.Transaction = GTXTransaction.Decode(gtxBytes);
            return newGTXObject;
        }
    }
}