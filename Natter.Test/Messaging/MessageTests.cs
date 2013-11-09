using System;
using NUnit.Framework;
using Natter.Byte;
using Natter.Messaging;

namespace Natter.Test.Messaging
{
    [TestFixture]
    public class MessageTests
    {
        [Test]
        public void SerialiseMessage()
        {
            IField[] fields = new IField[] { new Field("Field1".GetBytes(), "Value".GetBytes()), new Field("Field2".GetBytes(), "AnotherValue".GetBytes()) };
            IMessage message = new Message("Type".GetBytes(), "Connection".GetBytes(), "Transaction".GetBytes(), "From".GetBytes(), fields);

            var res = message.Serialise();

            Assert.AreEqual("<Start-23,31,33,15,18,26><11,4>MessageType=Type <12,10>ConnectionId=Connection <13,11>TransactionId=Transaction <4,4>From=From <6,5>Field1=Value <6,12>Field2=AnotherValue ", res.GetString());
        }

        [Test]
        public void DeserialiseMessage()
        {
            IField[] fields = new IField[] { new Field("Field1".GetBytes(), "Value".GetBytes()), new Field("Field2".GetBytes(), "AnotherValue".GetBytes()) };
            IMessage message = new Message("Type".GetBytes(), "Connection".GetBytes(), "Transaction".GetBytes(), "From".GetBytes(), fields);

            message = Message.Deserialise(message.Serialise());

            Assert.AreEqual("Type", message.MessageType.GetString());
            Assert.AreEqual("Connection", message.ConnectionId.GetString());
            Assert.AreEqual("Transaction", message.TransactionId.GetString());
            Assert.AreEqual("From", message.From.GetString());
        }

        [Test]
        public void HowLong()
        {
            IField[] fields = new IField[] { new Field("Field1".GetBytes(), "Value".GetBytes()), new Field("Field2".GetBytes(), "AnotherValue".GetBytes()) };
            IMessage message = new Message("Type".GetBytes(), "Connection".GetBytes(), "Transaction".GetBytes(), "From".GetBytes(), fields);

            for (int loop = 0; loop < 10000; loop++)
            {
                Message.Deserialise(message.Serialise());
            }
        }
    }
}
