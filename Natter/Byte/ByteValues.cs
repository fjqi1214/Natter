namespace Natter.Byte
{
    public static class ByteValues
    {
        public static readonly byte GreaterThan = ">".GetBytes()[0];
        public static readonly byte LessThan = "<".GetBytes()[0];
        public static readonly byte Comma = ",".GetBytes()[0];
        public static readonly byte Equal = "=".GetBytes()[0];

        public static readonly byte[] GreaterThanArray = ">".GetBytes();
        public static readonly byte[] LessThanArray = "<".GetBytes();
        public static readonly byte[] CommaArray = ",".GetBytes();
        public static readonly byte[] EqualArray = "=".GetBytes();
        public static readonly byte[] SpaceArray = " ".GetBytes();

        public static readonly byte[] StartMessage = "<Start-".GetBytes();
        public static readonly byte[] EndMessage = "<End>".GetBytes();

        public static readonly byte[] MessageType = "MessageType".GetBytes();
        public static readonly byte[] ConnectionId = "ConnectionId".GetBytes();
        public static readonly byte[] TransactionId = "TransactionId".GetBytes();
        public static readonly byte[] From = "From".GetBytes();

    }
}
