using System.Linq;
using System;
using System.Collections.Generic;

namespace Chromia.Postchain.Client
{

    public class Gtx
    {
        public string BlockchainID {get; private set;}
        public List<Operation> Operations {get; private set;}
        public List<byte[]> Signers {get; private set;}
        public List<byte[]> Signatures {get; private set;}

        public Gtx(string blockchainRID): this()
        {
            this.BlockchainID = blockchainRID;

        }

        private Gtx()
        {
            this.BlockchainID = "";
            this.Operations = new List<Operation>();
            this.Signers = new List<byte[]>();
            this.Signatures = new List<byte[]>();
        }

        public Gtx AddOperationToGtx(string opName, object[] args)
        {
            if(this.Signatures.Count != 0)
            {
                throw new Exception("Cannot add function calls to an already signed gtx");
            }

            this.Operations.Add(new Operation(opName, args));
   
            return this;
        }

        public static GTXValue ArgToGTXValue(object arg)
        {
            var gtxValue = new GTXValue();
            
            if (arg is null)
            {
                gtxValue.Choice = GTXValueChoice.Null;
            }
            else if (PostchainUtil.IsNumericType(arg))
            {
                try
                {
                    gtxValue.Choice = GTXValueChoice.Integer;
                    gtxValue.Integer = Convert.ToInt64(arg);
                }
                catch
                {
                    throw new System.Exception("Chromia.PostchainClient.GTX Gtx.ArgToGTXValue() Integer overflow.");
                }                
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
            else if (arg is object[])
            {
                var array = (object[]) arg;
                gtxValue.Choice = GTXValueChoice.Array;

                gtxValue.Array = new List<GTXValue>();
                foreach (var subArg in array)
                {
                    gtxValue.Array.Add(ArgToGTXValue((object) subArg));
                }
            }
            else if (arg is Dictionary<string, object>)
            {
                gtxValue.Choice = GTXValueChoice.Dict;

                var dict = (Dictionary<string, object>) arg;

                gtxValue.Dict = new List<DictPair>();
                foreach (var dictPair in dict)
                {
                    gtxValue.Dict.Add(new DictPair(dictPair.Key, ArgToGTXValue(dictPair.Value)));
                }
            }
            else if (arg is Operation)
            {
                return ((Operation) arg).ToGtxValue();
            }
            else
            {
                throw new System.Exception("Chromia.PostchainClient.GTX Gtx.ArgToGTXValue() Can't create GTXValue out of type " + arg.GetType());
            }


            return gtxValue;
        }

        public void AddSignerToGtx(byte[] signer)
        {
            if(this.Signatures.Count != 0)
            {
                throw new Exception("Cannot add signers to an already signed gtx");
            }

            this.Signers.Add(signer);
        }

        public void Sign(byte[] privKey, byte[] pubKey)
        {
            byte[] bufferToSign = this.GetBufferToSign();
            var signature = PostchainUtil.Sign(bufferToSign, privKey);
            
            this.AddSignature(pubKey, signature);
        }

        public byte[] GetBufferToSign()
        {
            var encodedBuffer = Gtv.Hash(GetGtvTxBody());

            return encodedBuffer;
        }

        private object[] GetGtvTxBody()
        {
            var body = new List<object>();
            body.Add(PostchainUtil.HexStringToBuffer(this.BlockchainID));
            body.Add((from Operation op in this.Operations select op.Raw()).ToArray());
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

            var signerIndex = this.Signers.FindIndex(signer => signer.SequenceEqual(pubKeyBuffer));

            if (signerIndex == -1) {
                throw new Exception("No such signer, remember to call addSignerToGtx() before adding a signature");
            }

            this.Signatures[signerIndex] = signatureBuffer;
        }

        public string Serialize()
        {
            return PostchainUtil.ByteArrayToString(Encode());
        }

        public byte[] Encode()
        {
            var gtxBody = new List<object>();

            gtxBody.Add(GetGtvTxBody());
            gtxBody.Add(this.Signatures.ToArray());

            return Gtx.ArgToGTXValue(gtxBody.ToArray()).Encode();
        }

        public static Gtx Decode(byte[] encodedMessage)
        {
            var gtx = new Gtx();
            var gtxTransaction = new ASN1.AsnReader(encodedMessage);
            var gtxValue = GTXValue.Decode(gtxTransaction);
            var gtxPayload = gtxValue.Array[0];

            gtx.BlockchainID = PostchainUtil.ByteArrayToString(gtxPayload.Array[0].ByteArray);

            foreach (var opArr in gtxPayload.Array[1].Array)
            {
                var op = opArr.ToObjectArray();

                var opName = opArr.Array[0].String;
                var opArgs = opArr.Array[1].ToObjectArray();
                gtx.AddOperationToGtx(opName, opArgs);
            }

            foreach (var signer in gtxPayload.Array[2].Array)
            {
                gtx.AddSignerToGtx(signer.ByteArray);
            }

            foreach (var sig in gtxValue.Array[1].Array)
            {
                gtx.Signatures.Add(sig.ByteArray);
            }

            return gtx;
        }
    }
}