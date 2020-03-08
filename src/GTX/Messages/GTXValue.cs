using System.Linq;
using System.Collections.Generic;
using System;

namespace Chromia.Postchain.Client
{    
    public enum GTXValueChoice
    {
        NotSet = -1,
        Null = 0,
        ByteArray = 1,
        String = 2,
        Integer = 3,
        Dict = 4,
        Array = 5
    }
    public class GTXValue
    {
        public GTXValueChoice Choice;
        public byte[] ByteArray;
        public string String;
        public long Integer;
        public List<DictPair> Dict = null;
        public List<GTXValue> Array = null;

        public GTXValue()
        {
            this.Choice = GTXValueChoice.NotSet;
        }

        public override bool Equals(object obj)
        {
            if ((obj == null) || ! this.GetType().Equals(obj.GetType())) 
            {
                return false;
            }
            else { 
                GTXValue gtxValue = (GTXValue) obj;
                
                return this.Choice.Equals(gtxValue.Choice) 
                    && ((this.ByteArray == null || gtxValue.ByteArray == null) ? this.ByteArray == gtxValue.ByteArray : Enumerable.SequenceEqual(this.ByteArray, gtxValue.ByteArray))
                    && this.Integer.Equals(gtxValue.Integer)
                    && ((this.Dict == null || gtxValue.Dict == null) ? this.Dict == gtxValue.Dict : Enumerable.SequenceEqual(this.Dict, gtxValue.Dict))
                    && ((this.Array == null || gtxValue.Array == null) ? this.Array == gtxValue.Array : Enumerable.SequenceEqual(this.Array, gtxValue.Array));
            }   
        }

        public override int GetHashCode()
        {
            return Choice.GetHashCode()
                + ByteArray.GetHashCode()
                + Integer.GetHashCode()
                + Dict.GetHashCode()
                + Array.GetHashCode();
        }

        public byte[] Encode()
        {
            var messageWriter = new AsnWriter();

            var choiceSize = 0;
            var choiceConstants = new List<byte>();
            switch (this.Choice)
            {
                case (GTXValueChoice.Null):
                {
                    choiceConstants.Add(0xa0);
                    messageWriter.WriteNull();
                    break;
                } 
                // The CHOICE in Asn1 is not implement in the used (experimental) library, yet.
                // Therefore we have to hack around a bit and create the bytes manually.
                // Since we can't seem to access the standard, we observed that the 2 octets are structured as follow:
                // |--0xa--| |--type--| |----length----|
                case (GTXValueChoice.ByteArray):
                {
                    choiceConstants.Add(0xa1);

                    messageWriter.WriteOctetString(this.ByteArray);
                    break;
                }
                case (GTXValueChoice.String):
                {
                    choiceConstants.Add(0xa2);

                    messageWriter.WriteUTF8String(this.String);
                    break;
                }
                case (GTXValueChoice.Integer):
                {
                    choiceConstants.Add(0xa3);

                    messageWriter.WriteInteger(this.Integer); 
                    break;
                }
                case (GTXValueChoice.Array):
                {
                    choiceConstants.Add(0xa5);

                    messageWriter.PushSequence();
                    foreach (var gtxValue in this.Array)
                    {
                        messageWriter.WriteEncodedValue(gtxValue.Encode());
                    }
                    messageWriter.PopSequence();
                    break;
                }
                case (GTXValueChoice.Dict):
                {
                    choiceConstants.Add(0xa4);

                    messageWriter.PushSequence();
                    foreach (var dictPair in this.Dict)
                    {
                        messageWriter.WriteEncodedValue(dictPair.Encode());
                    }
                    messageWriter.PopSequence();
                    break;
                }
                default:
                {
                    throw new System.Exception("Chromia.PostchainClient.GTX.Messages GTXValue.Encode() GTXValueChoice.Default case. Unknown choice " + this.Choice);
                }
            }
            
            choiceSize = messageWriter.GetEncodedLength();
            if (choiceSize < 128)
            {
                choiceConstants.Add((byte) choiceSize);
            }
            else
            {
                var sizeInBytes = TrimByteList(BitConverter.GetBytes(choiceSize));
                
                var sizeLength = (byte) sizeInBytes.Length;

                choiceConstants.Add((byte) (0x80 + sizeLength));
                if (BitConverter.IsLittleEndian)
                {
                    sizeInBytes = sizeInBytes.Reverse().ToArray();
                }
                choiceConstants.AddRange(sizeInBytes);
            }
            
            return choiceConstants.ToArray().Concat(messageWriter.Encode()).ToArray();
 
        }

        private static byte[] TrimByteList(byte[] byteList)
        {
            List<byte> trimmedBytes = new List<byte>();
            for (int i = byteList.Length - 1; i >= 0; i--)
            {
                if (byteList[i] != 0)
                {
                    for (int j = 0; j <= i; j++)
                    {
                        trimmedBytes.Add(byteList[j]);
                    }

                    break;
                }
            }

            return trimmedBytes.ToArray();
        }

        public object[] ToObjectArray()
        {
            if (Choice != GTXValueChoice.Array)
            {
                throw new Exception("Tried to cast non array choice to object array.");
            }

            List<object> retArr = new List<object>();

            foreach(var innerGtxValue in Array)
            {
                switch (innerGtxValue.Choice)
                {
                    case (GTXValueChoice.ByteArray):
                    {
                        retArr.Add(innerGtxValue.ByteArray);
                        break;
                    }
                    case (GTXValueChoice.String):
                    {
                        retArr.Add(innerGtxValue.String);
                        break;
                    }
                    case (GTXValueChoice.Integer):
                    {
                        retArr.Add(innerGtxValue.Integer);
                        break;
                    }
                    case (GTXValueChoice.Array):
                    {
                        retArr.Add(innerGtxValue.ToObjectArray());
                        break;
                    }
                    case (GTXValueChoice.Dict):
                    {
                        throw new Exception("Unsupported type Dict.");
                    }
                    default:
                    {
                        throw new Exception("Unknown GTXValue choice " + innerGtxValue.Choice);
                    }
                }
            }

            return retArr.ToArray();
        }

        [Obsolete("Use ASN1Writer.GetEncodedLength() instead")]
        private static byte GetValueSize(GTXValue gtxValue)
        {
            switch (gtxValue.Choice)
            {
                case (GTXValueChoice.ByteArray):
                {
                    byte size = (byte) (gtxValue.ByteArray.Length + 2);
                    if (size > 127)
                    {
                        size += 1;
                    }
                    
                    return size;
                }
                case (GTXValueChoice.String):
                {
                    byte size = (byte) (gtxValue.String.Length + 2);
                    if (size > 127)
                    {
                        size += 1;
                    }
                    
                    return size;
                }
                case (GTXValueChoice.Integer):
                {       
                    byte size = (byte) (PostchainUtil.GetMaxAmountOfBytesForInteger(gtxValue.Integer) + 2);
                    if (size > 127)
                    {
                        size += 1;
                    }
                    
                    return size;
                }
                case (GTXValueChoice.Array):
                {
                    byte choiceSize = (byte) 2;

                    foreach (var val in gtxValue.Array)
                    {
                        var tmpSize = GetValueSize(val);

                        if (tmpSize > 127)
                        {
                            choiceSize += (byte) (tmpSize + 3);
                        }
                        else
                        {
                            choiceSize += (byte) (tmpSize + 2);
                        }
                    }
                    
                    if (choiceSize > 127)
                    {
                        choiceSize += 1;
                    }

                    return choiceSize;
                }
                case (GTXValueChoice.Dict):
                {
                    byte choiceSize = (byte) 2;

                    foreach (var val in gtxValue.Dict)
                    {
                        var tmpSize = (byte) ((val.Name.Length + 2) + GetValueSize(val.Value));

                        if (tmpSize > 127)
                        {
                            choiceSize += (byte) (tmpSize + 5);
                        }
                        else
                        {
                            choiceSize += (byte) (tmpSize + 4);
                        }
                    }

                    if (choiceSize > 127)
                    {
                        choiceSize += 1;
                    }

                    return choiceSize;
                }
                default:
                {
                    throw new System.Exception("Chromia.PostchainClient.GTX.Messages GTXValue.GetValueSize() GTXValueChoice.Default case. Unknown choice " + gtxValue.Choice);
                }
            }
        }

        public override string ToString()
        {
            switch (Choice)
            {
                case (GTXValueChoice.Null):
                {
                    return "null";
                }
                case (GTXValueChoice.ByteArray):
                {
                    return PostchainUtil.ByteArrayToString(ByteArray);
                }
                case (GTXValueChoice.String):
                {
                    return String;
                }
                case (GTXValueChoice.Integer):
                {      
                    return Integer.ToString();
                }
                case (GTXValueChoice.Array):
                {
                    string ret = "[";
                    if (Array.Count == 0)
                    {
                        return ret + "]";
                    }

                    foreach(var elm in Array)
                    {
                        ret += elm.ToString() + ", ";
                    }

                    return ret.Remove(ret.Length - 2) + "]";
                }
                case (GTXValueChoice.Dict):
                {
                    string ret = "[";
                    if (Dict.Count == 0)
                    {
                        return ret + "]";
                    }

                    foreach(var elm in Dict)
                    {
                        ret += @"{{""" + elm.Name + @""": " + elm.Value.ToString() + "}, ";
                    }

                    return ret.Remove(ret.Length - 2) + "]";
                }
                default:
                {
                    return "";
                }
            }
        }
    }
}