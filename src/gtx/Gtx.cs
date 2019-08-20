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
        public static Gtx AddTransactionToGtx(string opName, Gtx gtx, params object[] args)
        {
            if(gtx == null)
            {
                throw new Exception("No Gtx to add operation to");
            }

            if(gtx.signers.Count != 0)
            {
                throw new Exception("Cannot add function calls to an already signed gtx");
            }

            gtx.operations.Add(new Operation(opName, args));
   
            return gtx;
        }

        public static void AddSignerToGtx(byte[] signer, Gtx gtx)
        {
            if(gtx.signers.Count != 0)
            {
                throw new Exception("Cannot add signers to an already signed gtx");
            }

            gtx.signers.Add(signer);
        }

        public static void Sign(byte[] privKey, byte[] pubKey, Gtx gtx)
        {
            byte[] bufferToSign = Gtx.GetBufferToSign(gtx);
            var signature = Util.Sign(bufferToSign, privKey);
            Console.WriteLine("PubKey: " + BitConverter.ToString(pubKey));
            Console.WriteLine("Signature: " + BitConverter.ToString(signature));
            Gtx.AddSignature(pubKey, signature, gtx);
        }

        /**
        * Serializes the gtx for signing
        * @param gtx the gtx to serialize
        */
        public static byte[] GetBufferToSign(Gtx gtx)
        {
            //return serialization.encode({blockchainRID: gtx.blockchainRID, operations: gtx.operations, signers: gtx.signers, signatures: []});
            return null;
        }

        public static void AddSignature(byte[] pubKeyBuffer, byte[] signatureBuffer, Gtx gtx)
        {
            if(gtx.signatures == null) {
                gtx.signatures = new List<byte[]>();
            }

            /* ?
            if (gtx.signers.length != gtx.signatures.length) {
                throw new Error("Mismatching signers and signatures");
            } 
            */

            var signerIndex = gtx.signers.IndexOf(pubKeyBuffer);

            if (signerIndex == -1) {
                throw new Exception("No such signer, remember to call addSignerToGtx() before adding a signature");
            }

            gtx.signatures[signerIndex] = signatureBuffer;
        }

        public static string Serialize(Gtx gtx)
        {
           if (gtx.signatures == null)
           {
                gtx.signatures = new List<byte[]>();
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