namespace Chromia.Postchain.Client
{
    public class DictPair
    {
        public string Name;
        public GTXValue Value;

        public DictPair(string name = "", GTXValue value = null)
        {
            this.Name = name;

            if (value == null)
            {
                this.Value = new GTXValue();
            }
            else
            {
                this.Value = value;
            }
        }

        public override bool Equals(object obj)
        {
            if ((obj == null) || ! this.GetType().Equals(obj.GetType())) 
            {
                return false;
            }
            else { 
                DictPair dictPair = (DictPair) obj;
                
                return this.Name.Equals(dictPair.Name)
                    && this.Value.Equals(dictPair.Value);
            }   
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode()
                + Value.GetHashCode();
        }

        public byte[] Encode()
        {
            var messageWriter = new ASN1.AsnWriter();
            
            messageWriter.PushSequence();
            messageWriter.WriteUTF8String(this.Name);
            messageWriter.WriteEncodedValue(Value.Encode());
            messageWriter.PopSequence();

            return messageWriter.Encode();
        }

        public static DictPair Decode(ASN1.AsnReader sequence)
        {
            var dict = new DictPair();
            var dictSequence = sequence.ReadSequence();

            dict.Name = dictSequence.ReadUTF8String();
            dict.Value = GTXValue.Decode(dictSequence);

            return dict;
        }
    }
}