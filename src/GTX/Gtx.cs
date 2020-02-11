using System.Linq;
using System;
using Chromia.Postchain.Client.GTX.ASN1Messages;
using System.Collections.Generic;

namespace Chromia.Postchain.Client.GTX
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

            if (args is null) {
                args = new dynamic[]{null};
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
            else if (ASN1Util.IsNumericType(arg))
            {
                try
                {
                    gtxValue.Choice = GTXValueChoice.Integer;
                    gtxValue.Integer = Convert.ChangeType(arg, typeof(long));
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
            else if (arg.GetType().IsArray)
            {
                gtxValue.Choice = GTXValueChoice.Array;

                gtxValue.Array = new List<GTXValue>();
                foreach (var subArg in arg)
                {
                    gtxValue.Array.Add(ArgToGTXValue((dynamic) subArg));
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
            var signature = Util.Sign(bufferToSign, privKey);
            
            this.AddSignature(pubKey, signature);
        }

        public byte[] GetBufferToSign()
        {
            var oldSignatures = this.Signatures;
            this.Signatures.Clear();

            var encodedBuffer = Chromia.Postchain.Client.GTV.Gtv.Hash(GetGtvTxBody(true));

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

        public static GTXValue Deserialize(byte[] encodedMessage)
        {
            if (encodedMessage[0] >> 4 != 0xa)
            {
                return new GTXValue();
            }

            var messageLength = GetLength(encodedMessage);
            int messageOctetLength = GetOctetLength(encodedMessage);
            var newObject = new GTXValue();
            switch (encodedMessage[0] & 0xF)
            {          
                case (0x1):
                {
                    // ByteArray
                    if (encodedMessage[1+messageOctetLength] != 0x04)
                    {
                        throw new System.Exception("Chromia.Postchain.Client.GTX Gtx.Deserialize() ByteArray case. Not octet string.");
                    }

                    int length = encodedMessage[3];

                    newObject.Choice = GTXValueChoice.ByteArray;
                    newObject.ByteArray = encodedMessage.Skip(4).Take(length).ToArray();

                    break;
                }
                case (0x2):
                {
                    // String
                    if (encodedMessage[1+messageOctetLength] != 0x0c)
                    {
                        throw new System.Exception("Chromia.Postchain.Client.GTX Gtx.Deserialize() String case. Not UTF8String.");
                    }

                    int length = encodedMessage[1+messageOctetLength+1];

                    newObject.Choice = GTXValueChoice.String;
                    newObject.String = System.Text.Encoding.UTF8.GetString(encodedMessage.Skip(4).Take(length).ToArray());
                    break;
                }
                case (0x3):
                {
                    // Integer
                    if (encodedMessage[1+messageOctetLength] != 0x02)
                    {
                        throw new System.Exception("Chromia.Postchain.Client.GTX Gtx.Deserialize() Integer case. Not primitive integer type.");
                    }

                    int length = encodedMessage[3];
                    int newInteger = 0;
                    for (int i = 4; i < length + 4; i++)
                    {
                        newInteger = (newInteger << 8) | encodedMessage[i];
                    }

                    newObject.Choice = GTXValueChoice.Integer;
                    newObject.Integer = newInteger;

                    break;
                }
                case (0x4):
                {
                    // DictPair
                    throw new System.Exception("Chromia.Postchain.Client.GTX Gtx.Deserialize() Dict case. Not implemented yet.");
                    // newObject.Choice = GTXValueChoice.Dict;
                    // break;
                } 
                case (0x5):
                {
                    // Array
                    if (encodedMessage[1+messageOctetLength] != 0x30)
                    {
                        throw new System.Exception("Chromia.Postchain.Client.GTX Gtx.Deserialize() Array case. Not sequence of.");
                    }

                    byte[] sequence = encodedMessage.Skip(1+messageOctetLength).ToArray();
                    int sequenceLength = GetLength(sequence);
                    int sequenceOctetLength = GetOctetLength(sequence);
                    
                    int startIndex = 2 + messageOctetLength + sequenceOctetLength;

                    newObject.Choice = GTXValueChoice.Array;
                    newObject.Array = new List<GTXValue>();
                    while (startIndex < sequenceLength)
                    {
                        byte[] newElement = encodedMessage.Skip(startIndex).ToArray();
                        int elementLength = GetLength(newElement) + GetOctetLength(newElement) + 1;
                        newElement = newElement.Take(elementLength).ToArray();

                        newObject.Array.Add(Deserialize(newElement));
                        startIndex += elementLength;
                    }
                    
                    break;
                }      
                default:
                {
                    throw new System.Exception("Chromia.Postchain.Client.GTX Gtx.Deserialize() Default case. Unknown tag " + (encodedMessage[0] & 0xF).ToString("X1"));
                }
            }

            return newObject;
        }

        private static int GetLength(byte[] encodedMessage)
        {
            byte octetLength = GetOctetLength(encodedMessage);
            if (octetLength > 1)
            {
                int length = 0;
                for (int i = 2; i < octetLength + 1; i++)
                {
                    length = length << 8 | encodedMessage[i];
                }
                return length;
            }
            else
            {
                return encodedMessage[1];
            }
        }

        private static byte GetOctetLength(byte[] encodedMessage)
        {
            if ((encodedMessage[1] & 0x80) != 0)
            {
                return (byte) ((encodedMessage[1] & (~((byte)0x80))) + 1);
            }
            else
            {
                return 1;
            }
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