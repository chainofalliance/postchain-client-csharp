using System.Collections.Generic;
using System.Linq;

namespace Chromia.Postchain.Client
{
    [System.Obsolete("Not used any more", true)]
    internal class GTXOperation
    {
        public string OpName;
        public List<GTXValue> Args;

        public GTXOperation(string opName = "")
        {
            this.OpName = opName;
            this.Args = new List<GTXValue>();
        }

        public override bool Equals(object obj)
        {
            if ((obj == null) || ! this.GetType().Equals(obj.GetType())) 
            {
                return false;
            }
            else { 
                GTXOperation gtxOperation = (GTXOperation) obj;
                
                return this.OpName.Equals(gtxOperation.OpName)
                    && ((this.Args == null || gtxOperation.Args == null) ? this.Args == gtxOperation.Args : Enumerable.SequenceEqual(this.Args, gtxOperation.Args));
            }   
        }

        public override int GetHashCode()
        {
            return OpName.GetHashCode();
        }

        public byte[] Encode()
        {
            var messageWriter = new AsnWriter();
            messageWriter.PushSequence();

            messageWriter.WriteUTF8String(this.OpName);

            messageWriter.PushSequence();
            if (this.Args.Count > 0)
            {
                foreach(var arg in this.Args)
                {
                    messageWriter.WriteEncodedValue(arg.Encode());
                }
            }
            messageWriter.PopSequence();

            messageWriter.PopSequence();
            return messageWriter.Encode();
        }

        // public static GTXOperation Decode(byte[] encodedMessage)
        // {
        //     var gtxOperation = new AsnReader(encodedMessage, AsnEncodingRules.BER);
        //     var gtxOperationSequence = gtxOperation.ReadSequence();

        //     var newObject = new GTXOperation();
        //     newObject.OpName = gtxOperationSequence.ReadCharacterString(UniversalTagNumber.UTF8String);

        //     var valueSequence = gtxOperationSequence.ReadSequence();
        //     newObject.Args = ASN1Util.SequenceToList<GTXValue>(valueSequence, GTXValue.Decode);

        //     return newObject;
        // }
    }
}