﻿using System;
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

            _client1.OnConnected(() => reset1.Set()).OnData(f => HandleResponse(f, _client1)).Call(GetClient2Address());
            _client2.OnConnected(() => reset2.Set()).OnData(f => HandleResponse(f, _client2)).Listen();
            Assert.IsTrue(reset1.WaitOne(TimeSpan.FromSeconds(5)), "Failed to connect");
            Assert.IsTrue(reset2.WaitOne(TimeSpan.FromSeconds(5)), "Failed to connect");

            reset1.Reset();
            reset2.Reset();
            _client1.OnDisconnected(() => reset1.Set());
            _client2.OnDisconnected(() => reset2.Set());
            Send(_client1, StartNumber);

            Assert.IsTrue(reset1.WaitOne(TimeSpan.FromSeconds(20)), "Failed to disconnect");
            Assert.IsTrue(reset2.WaitOne(TimeSpan.FromSeconds(20)), "Failed to disconnect");
            Assert.AreEqual(ConnectionState.Disconnected, _client2.State, "Client not disconnected");
            Assert.AreEqual(ConnectionState.Disconnected, _client1.State, "Client not disconnected");

            Assert.AreEqual(EndNumber, _lastResult, "Invalid last number");
            Assert.AreEqual((EndNumber - StartNumber) + 1, _count, "Invalid count");
        }

        private void Send(INatterClient client, int num)
        {
            var data = new IField[] { new Field(DataField.GetBytes(), num.ToString().GetBytes()) };
            client.Send(data);
        }

        private void HandleResponse(IField[] data, INatterClient client)
        {
            if (data.Length == 1 && data[0].Name.GetString() == DataField)
            {
                _count++;
                _lastResult = int.Parse(data[0].Value.GetString());
                if (_lastResult == EndNumber)
                {
                    client.End();
                }
                else
                {
                    Send(client, _lastResult + 1);
                }
            }
        }

        protected abstract INatterClient GetClient1();
        protected abstract INatterClient GetClient2();

        protected abstract IAddress GetClient2Address();
    }
}