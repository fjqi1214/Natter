namespace Natter.Messaging
{

    /* <Start-30,16><12,12>ConnectionId=23423 143243<12,2>TransactionId=23
     * 
     * 
     * 
     * 
     * 
     */



    public interface IMessage
    {
        byte[] MessageType { get; }
        byte[] ConnectionId { get; }
        byte[] TransactionId { get; }
        byte[] From { get; }
        IField[] Data { get; }

        byte[] Serialise();
    }
}
