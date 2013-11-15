using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using NUnit.Framework;
using Natter.Byte;
using Natter.Client;
using Natter.Connecting;
using Natter.Messaging;
using Natter.Transporting;

namespace Natter.Test.Communicating
{
    public abstract class CommunicationTest
    {
        private INatterClient _client1;
        private INatterClient _client2;
        private int _count = 0;
        private int _lastResult = 0;

        private const string DataField = "Data";
        private const int StartNumber = 0;
        private const int EndNumber = 1000;

        [SetUp]
        public void Setup()
        {
            _client1 = GetClient1();
            _client2 = GetClient2();
        }

        [TearDown]
        public void TearDown()
        {
            _client1.Dispose();
            _client2.Dispose();
        }

        [Test]
        public void SimpleCommunication()
        {
            var reset1 = new ManualResetEvent(false);
            var reset2 = new ManualResetEvent(false);

            INatterConnection connection2 = null;
            var connection1 =_client1.OnConnected(c => reset1.Set()).OnData((c, f) => HandleResponse(f, c)).Call(GetClient2Address());
            _client2.OnConnected(c => reset2.Set()).OnData((c, f) => { HandleResponse(f, c); connection2 = c; });
            Assert.IsTrue(reset1.WaitOne(TimeSpan.FromSeconds(5)), "Failed to connect");
            Assert.IsTrue(reset2.WaitOne(TimeSpan.FromSeconds(5)), "Failed to connect");

            reset1.Reset();
            reset2.Reset();
            _client1.OnDisconnected(c => reset1.Set());
            _client2.OnDisconnected(c => reset2.Set());
            Send(connection1, StartNumber);

            Assert.IsTrue(reset1.WaitOne(TimeSpan.FromSeconds(20)), "Failed to disconnect");
            Assert.IsTrue(reset2.WaitOne(TimeSpan.FromSeconds(20)), "Failed to disconnect");
            Assert.AreEqual(ConnectionState.Disconnected, connection1.State, "Client not disconnected");
            Assert.AreEqual(ConnectionState.Disconnected, connection2.State, "Client not disconnected");

            Assert.AreEqual(EndNumber, _lastResult, "Invalid last number");
            Assert.AreEqual((EndNumber - StartNumber) + 1, _count, "Invalid count");
        }

        private void Send(INatterConnection connection, int num)
        {
            var data = new IField[] { new Field(DataField.GetBytes(), num.ToString().GetBytes()) };
            connection.Send(data);
        }

        private void HandleResponse(IField[] data, INatterConnection connection)
        {
            if (data.Length == 1 && data[0].Name.GetString() == DataField)
            {
                _count++;
                _lastResult = int.Parse(data[0].Value.GetString());
                if (_lastResult == EndNumber)
                {
                    connection.Close();
                }
                else
                {
                    Send(connection, _lastResult + 1);
                }
            }
        }

        protected abstract INatterClient GetClient1();
        protected abstract INatterClient GetClient2();

        protected abstract IAddress GetClient2Address();
    }
}
