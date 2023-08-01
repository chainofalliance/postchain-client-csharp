namespace Chromia.Encoding
{
    internal enum Asn1Choice
    {
        None = 0,
        Null = 0xa0,
        ByteArray = 0xa1,
        String = 0xa2,
        Integer = 0xa3,
        Dict = 0xa4,
        Array = 0xa5,
        BigInteger = 0xa6
    }

    internal enum Asn1Tag
    {
        Null = 0x05,
        OctetString = 0x04,
        UTF8String = 0x0c,
        Integer = 0x02,
        Sequence = 0x30
    }
}