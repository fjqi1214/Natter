using System;
using Natter.Byte;

namespace Natter.Messaging
{
    public class Message : IMessage
    {
        private IField _messageType = null;
        public byte[] MessageType
        {
            get { return _messageType != null ? _messageType.Value : new byte[0]; }
        }

        private IField _connectionId;
        public byte[] ConnectionId
        {
            get { return _connectionId != null ? _connectionId.Value : new byte[0]; }
        }

        private IField _transactionId;
        public byte[] TransactionId
        {
            get { return _transactionId != null ? _transactionId.Value : new byte[0]; }
        }

        private IField _from;
        public byte[] From
        {
            get { return _from != null ? _from.Value : new byte[0]; }
        }

        public IField[] Data
        {
            get;
            private set;
        }

        public Message(byte[] messageType, byte[] connectionId, byte[] transactionId, byte[] from, IField[] fields)
            : this(new Field(ByteValues.MessageType, messageType), new Field(ByteValues.ConnectionId, connectionId),
            new Field(ByteValues.TransactionId, transactionId), new Field(ByteValues.From, from), fields)
        {
        }

        public Message(IField messageType, IField connectionId, IField transactionId, IField from, IField[] data)
        {
            _messageType = messageType;
            _connectionId = connectionId;
            _transactionId = transactionId;
            _from = from;
            Data = data;
        }

        public byte[] Serialise()
        {
            return SerialiseMessage(_messageType, _connectionId, _transactionId, _from, Data);
        }

        public static IMessage Deserialise(byte[] data)
        {
            return DeserialiseMessage(data);
        }

        private static byte[] SerialiseMessage(IField messageType, IField connectionId, 
            IField transactionId, IField from, IField[] data)
        {
            var allFields = new IField[data.Length + 4];
            allFields[0] = messageType;
            allFields[1] = connectionId;
            allFields[2] = transactionId;
            allFields[3] = from;
            Array.Copy(data, 0, allFields, 4, data.Length);

            byte[][] fields = new byte[allFields.Length + 1][];
            for (int loop = 0; loop < allFields.Length; loop++)
            {
                fields[loop + 1] = allFields[loop].Serialise();
            }

            var start = ByteTools.Combine(ByteValues.StartMessage,
                GetFieldLengths(fields),
                ByteValues.GreaterThanArray);
            fields[0] = start;

            return ByteTools.Combine(fields);
        }

        private static byte[] GetFieldLengths(byte[][] fields)
        {
            bool useComma = fields.Length > 1;
            int lastField = fields.Length - 1;

            byte[] data = new byte[0];
            for (int loop = 0; loop < fields.Length; loop++)
            {
                var field = fields[loop];
                if (field != null)
                {
                    var length = field.Length.ToString().GetBytes();

                    if (useComma && loop != lastField)
                    {
                        data = ByteTools.Combine(data, length, ByteValues.CommaArray);
                    }
                    else
                    {
                        data = ByteTools.Combine(data, length);
                    }
                }
            }
            return data;
        }

        private static IMessage DeserialiseMessage(byte[] data)
        {
            if (!ByteTools.Compare(ByteValues.StartMessage, 0, data, 0, ByteValues.StartMessage.Length))
            {
                throw new ArgumentException("Invalid message data");
            }

            int[] fieldLengths = new int[0];
            int start = ByteValues.StartMessage.Length;
            for (int pos = start; pos < data.Length; pos++)
            {
                if (data[pos] == ByteValues.Comma || data[pos] == ByteValues.GreaterThan)
                {
                    int length = pos - start;
                    byte[] size = new byte[length];
                    Array.Copy(data, start, size, 0, length);
                    int fieldLength = int.Parse(size.GetString());
                    start = pos + 1;

                    int[] temp = new int[fieldLengths.Length + 1];
                    Array.Copy(fieldLengths, 0, temp, 0, fieldLengths.Length);
                    temp[fieldLengths.Length] = fieldLength;
                    fieldLengths = temp;

                    if (data[pos] == ByteValues.GreaterThan)
                    {
                        break;
                    }
                }
            }

            IField messageType = null;
            IField connectionId = null;
            IField transactionId = null;
            IField from = null;
            var dataFields = new IField[fieldLengths.Length - 4];
            int fieldsCount = 0;

            for (int loop = 0; loop < fieldLengths.Length; loop++)
            {
                byte[] fieldData = new byte[fieldLengths[loop]];
                Array.Copy(data, start, fieldData, 0, fieldLengths[loop]);
                IField field = Field.Deserialise(fieldData);
                if (messageType == null && ByteTools.Compare(field.Name, 0, ByteValues.MessageType, 0, field.Name.Length))
                {
                    messageType = field;
                }
                else if (connectionId == null && ByteTools.Compare(field.Name, 0, ByteValues.ConnectionId, 0, field.Name.Length))
                {
                    connectionId = field;
                }
                else if (transactionId == null && ByteTools.Compare(field.Name, 0, ByteValues.TransactionId, 0, field.Name.Length))
                {
                    transactionId = field;
                }
                else if (from == null && ByteTools.Compare(field.Name, 0, ByteValues.From, 0, field.Name.Length))
                {
                    from = field;
                }
                else
                {
                    dataFields[fieldsCount++] = field;
                }
                start += fieldLengths[loop];
            }

            return new Message(messageType, connectionId, transactionId, from, dataFields);
        }
    }
}
