using System.Collections.Generic;
using System;

namespace Chromia.PostchainClient
{

    public class Gtx
    {
        public string blockchainRID;
        public List<Operation> operations;
        public List<byte[]> signers;
        public List<byte[]> signatures;

        public Gtx(string blockchainRID)
        {
            this.blockchainRID = blockchainRID;
            this.operations = new List<Operation>();
            this.signers = new List<byte[]>();
            this.signatures = null;
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
        public Gtx AddTransactionToGtx(string opName, params object[] args)
        {
           if(this.signers.Count != 0)
            {
                throw new Exception("Cannot add function calls to an already signed gtx");
            }

            this.operations.Add(new Operation(opName, args));
   
            return this;
        }

        public void AddSignerToGtx(byte[] signer)
        {
            if(this.signers.Count != 0)
            {
                throw new Exception("Cannot add signers to an already signed gtx");
            }

            this.signers.Add(signer);
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
            //return serialization.encode({blockchainRID: gtx.blockchainRID, operations: gtx.operations, signers: gtx.signers, signatures: []});
            return null;
        }

        public void AddSignature(byte[] pubKeyBuffer, byte[] signatureBuffer)
        {
            if(this.signatures == null) {
                this.signatures = new List<byte[]>();
            }

            
            if (this.signers.Count != this.signatures.Count) {
                throw new Exception("Mismatching signers and signatures");
            } 
            

            var signerIndex = this.signers.IndexOf(pubKeyBuffer);

            if (signerIndex == -1) {
                throw new Exception("No such signer, remember to call addSignerToGtx() before adding a signature");
            }

            this.signatures[signerIndex] = signatureBuffer;
        }

        public string Serialize()
        {
           if (this.signatures == null)
           {
                this.signatures = new List<byte[]>();
           }

           //return serialization.encode(gtx);
           return null;
        }

        public static Gtx Deserialize(byte[] gtxBytes)
        {
           //return serialization.decode(gtxBytes);
           return null;
        }

    }

    public class Operation
    {
        public string _opName;
        public object[] _args;

        public Operation(string opName, params object[] args)
        {
            this._opName = opName;
            this._args = args;
        }
    }

}