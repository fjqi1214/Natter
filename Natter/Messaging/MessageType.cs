using Natter.Byte;

namespace Natter.Messaging
{
    public class MessageType
    {
        public static readonly MessageType Start = new MessageType("Start");
        public static readonly MessageType Ok = new MessageType("Ok");
        public static readonly MessageType Error = new MessageType("Error");
        public static readonly MessageType Ping = new MessageType("Ping");
        public static readonly MessageType Data = new MessageType("Data");
        public static readonly MessageType End = new MessageType("End");

        public static readonly MessageType[] All = new MessageType[] {
            Start,
            Ok,
            Error,
            Ping,
            Data,
            End };

        public string Name
        {
            get;
            private set;
        }

        public byte[] NameSerialised
        {
            get;
            private set;
        }

        private MessageType(string name)
        {
            Name = name;
            NameSerialised = name.GetBytes();
        }

        public static MessageType Parse(byte[] messageType)
        {
            MessageType type = null;
            for (int loop = 0; loop < All.Length; loop++)
            {
                if(ByteTools.Compare(messageType, 0, All[loop].NameSerialised, 0, All[loop].NameSerialised.Length))
                {
                    type = All[loop];
                }
            }
            return type;
        }

        public static IMessage CreateStartMessage(byte[] connectionId, byte[] transactionId, byte[] from)
        {
            return new Message(Start.NameSerialised, 
                connectionId, 
                transactionId,
                from, new IField[0]);
        }

        public static IMessage CreateOkMessage(byte[] connectionId, byte[] transactionId, byte[] from)
        {
            return new Message(Ok.NameSerialised,
                connectionId,
                transactionId,
                from, new IField[0]);
        }

        public static IMessage CreateEndMessage(byte[] connectionId, byte[] transactionId, byte[] from)
        {
            return new Message(End.NameSerialised,
                connectionId,
                transactionId,
                from, new IField[0]);
        }

        public static IMessage CreatePingMessage(byte[] connectionId, byte[] transactionId, byte[] from)
        {
            return new Message(Ping.NameSerialised,
                connectionId,
                transactionId,
                from, new IField[0]);
        }

        public static IMessage CreateDataMessage(byte[] connectionId, byte[] transactionId, byte[] from, IField[] data)
        {
            return new Message(Data.NameSerialised,
                connectionId,
                transactionId,
                from, 
                data);
        }

    }
}
