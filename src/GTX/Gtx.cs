using System.Linq;
using System;
using Chromia.PostchainClient.GTX.ASN1Messages;
using System.Collections.Generic;

namespace Chromia.PostchainClient.GTX
{

    public class Gtx
    {
        private string BlockchainID;
        private List<dynamic> Operations;
        private List<byte[]> Signers;
        private List<byte[]> Signatures;

        public Gtx(string blockchainRID)
        {
            this.BlockchainID = blockchainRID;
            this.Operations = new List<dynamic>();
            this.Signers = new List<byte[]>();
            this.Signatures = new List<byte[]>();
        }

        public Gtx AddOperationToGtx(string opName, dynamic[] args)
        {
           if(this.Signatures.Count != 0)
            {
                throw new Exception("Cannot add function calls to an already signed gtx");
            }

            var newOperation = new List<dynamic>(){opName, args};

            this.Operations.Add(newOperation.ToArray());
   
            return this;
        }

        public static GTXValue ArgToGTXValue(dynamic arg)
        {
            var gtxValue = new GTXValue();
            
            if (arg is null)
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

                gtxValue.Array = new List<GTXValue>();
                foreach (var subArg in arg)
                {
                    gtxValue.Array.Add(ArgToGTXValue(subArg));
                }
            }
            else if (arg is Dictionary<string, dynamic>)
            {
                gtxValue.Choice = GTXValueChoice.Dict;

                var dict = (Dictionary<string, dynamic>) arg;

                gtxValue.Dict = new List<DictPair>();
                foreach (var dictPair in dict)
                {
                    gtxValue.Dict.Add(new DictPair(dictPair.Key, ArgToGTXValue(dictPair.Value)));
                }
            }
            else
            {
                throw new System.Exception("Chromia.PostchainClient.GTX Gtx.ArgToGTXValue() Can't create GTXValue out of type " + arg.GetType());
            }


            return gtxValue;
        }

        public void AddSignerToGtx(byte[] signer)
        {
            if(this.Signers.Count != 0)
            {
                throw new Exception("Cannot add signers to an already signed gtx");
            }

            this.Signers.Add(signer);
        }

        public void Sign(byte[] privKey, byte[] pubKey)
        {
            byte[] bufferToSign = this.GetBufferToSign();
            Console.WriteLine("digestToSign: " + Util.ByteArrayToString(bufferToSign));
            var signature = Util.Sign(bufferToSign, privKey);
            Console.WriteLine("PubKey: " + Util.ByteArrayToString(pubKey));
            Console.WriteLine("Signature: " + Util.ByteArrayToString(signature));
            
            this.AddSignature(pubKey, signature);
        }

        public byte[] GetBufferToSign()
        {
            var oldSignatures = this.Signatures;
            this.Signatures.Clear();

            var encodedBuffer = Chromia.PostchainClient.GTV.Gtv.Hash(GetGtvTxBody(true));

            Console.WriteLine("ECNODED BUFFER: " + Util.ByteArrayToString(encodedBuffer));

            this.Signatures = oldSignatures;

            return encodedBuffer;
        }

        private dynamic[] GetGtvTxBody(bool asHexString = false)
        {
            var body = new List<dynamic>();
            body.Add(Util.HexStringToBuffer(this.BlockchainID));
            body.Add(this.Operations.ToArray());
            body.Add(this.Signers.ToArray());

            return body.ToArray();
        }

        public void AddSignature(byte[] pubKeyBuffer, byte[] signatureBuffer)
        {   
            if (this.Signatures.Count == 0)
            {
                foreach(var signer in this.Signers)
                {
                    this.Signatures.Add(null);
                }
            }

            if (this.Signers.Count != this.Signatures.Count) {
                throw new Exception("Mismatching signers and signatures");
            } 
            var signerIndex = this.Signers.FindIndex(signer => signer.SequenceEqual(pubKeyBuffer));

            if (signerIndex == -1) {
                throw new Exception("No such signer, remember to call addSignerToGtx() before adding a signature");
            }

            this.Signatures[signerIndex] = signatureBuffer;
        }

        public string Serialize()
        {
            var gtxBody = new List<dynamic[]>();

            gtxBody.Add(GetGtvTxBody());
            gtxBody.Add(this.Signatures.ToArray());
            
            return Util.ByteArrayToString(Gtx.ArgToGTXValue(gtxBody.ToArray()).Encode());
        }

        /*
        public static Gtx Deserialize(byte[] gtxBytes)
        {
            var newGTXObject = new Gtx();
            newGTXObject.Transaction = GTXTransaction.Decode(gtxBytes);
            return newGTXObject;
        }
        */
    }
}