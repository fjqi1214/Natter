using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using Natter.Client;
using Natter.Connecting;
using Natter.Transporting;

namespace Natter.Test.Connecting
{
    public abstract class ConnectionTest
    {
        [Test]
        public void Calling()
        {
            using (var client = GetClient2())
            {
                client.Call(GetClient1Address());

                Assert.AreEqual(ConnectionState.Calling, client.State, "Client not calling");
            }
        }

        [Test]
        public void Listening()
        {
            using (var client = GetClient1())
            {
                client.Listen();

                Assert.AreEqual(ConnectionState.Listening, client.State, "Client not listening");
            }
        }

        [Test]
        public void Connected()
        {
            var reset1 = new ManualResetEvent(false);
            var reset2 = new ManualResetEvent(false);

            var client1 = GetClient1();
            var client2 = GetClient2();
            try
            {
                client1.OnConnected(() => reset1.Set()).Listen();
                client2.OnConnected(() => reset2.Set()).Call(GetClient1Address());

                Assert.IsTrue(reset1.WaitOne(TimeSpan.FromSeconds(2)), "Failed to connect");
                Assert.IsTrue(reset2.WaitOne(TimeSpan.FromSeconds(2)), "Failed to connect");
                Assert.AreEqual(ConnectionState.Connected, client1.State, "Client not connected");
                Assert.AreEqual(ConnectionState.Connected, client2.State, "Client not connected");
            }
            finally
            {
                client1.Dispose();
                client2.Dispose();
            }
        }

        [Test]
        public void ConnectedInverted()
        {
            var reset1 = new ManualResetEvent(false);
            var reset2 = new ManualResetEvent(false);

            var client1 = GetClient1();
            var client2 = GetClient2();
            try
            {
                // Make the listen comes after the call (tests retry)
                client2.OnConnected(() => reset2.Set()).Call(GetClient1Address());
                Thread.Sleep(500);
                client1.OnConnected(() => reset1.Set()).Listen();

                Assert.IsTrue(reset1.WaitOne(TimeSpan.FromSeconds(4)), "Failed to connect");
                Assert.IsTrue(reset2.WaitOne(TimeSpan.FromSeconds(4)), "Failed to connect");
                Assert.AreEqual(ConnectionState.Connected, client1.State, "Client not connected");
                Assert.AreEqual(ConnectionState.Connected, client2.State, "Client not connected");
            }
            finally
            {
                client1.Dispose();
                client2.Dispose();
            }
        }

        [Test]
        public void Disconnected()
        {
            var reset1 = new ManualResetEvent(false);
            var reset2 = new ManualResetEvent(false);

            var client1 = GetClient1();
            var client2 = GetClient2();
            try
            {
                client1.OnConnected(() => reset1.Set()).Listen();
                client2.OnConnected(() => reset2.Set()).Call(GetClient1Address());

                Assert.IsTrue(reset1.WaitOne(TimeSpan.FromSeconds(2)), "Failed to connect");
                Assert.IsTrue(reset2.WaitOne(TimeSpan.FromSeconds(2)), "Failed to connect");

                reset1.Reset();
                reset2.Reset();
                client1.OnDisconnected(() => reset1.Set());
                client2.OnDisconnected(() => reset2.Set());
                client1.End();

                Assert.IsTrue(reset1.WaitOne(TimeSpan.FromSeconds(2)), "Failed to disconnect");
                Assert.IsTrue(reset2.WaitOne(TimeSpan.FromSeconds(2)), "Failed to disconnect");
                Assert.AreEqual(ConnectionState.Disconnected, client1.State, "Client not disconnected");
                Assert.AreEqual(ConnectionState.Disconnected, client2.State, "Client not disconnected");
            }
            finally
            {
                client1.Dispose();
                client2.Dispose();
            }
        }

        [Test]
        public void ConnectedTimeout()
        {
            var reset1 = new ManualResetEvent(false);
            var reset2 = new ManualResetEvent(false);

            var client1 = GetClient1();
            var client2 = GetClient2();
            try
            {
                client1.OnConnected(() => reset1.Set()).Listen();
                client2.OnConnected(() => reset2.Set()).Call(GetClient1Address());

                Assert.IsTrue(reset1.WaitOne(TimeSpan.FromSeconds(2)), "Failed to connect");
                Assert.IsTrue(reset2.WaitOne(TimeSpan.FromSeconds(2)), "Failed to connect");

                reset1.Reset();
                reset2.Reset();
                client2.OnError(e => reset1.Set()).OnDisconnected(() => reset2.Set());
                client1.Dispose();

                Assert.IsTrue(reset1.WaitOne(TimeSpan.FromSeconds(50)), "There was no error");
                Assert.IsTrue(reset2.WaitOne(TimeSpan.FromSeconds(50)), "Should be disconnected");
                Assert.AreEqual(ConnectionState.Disconnected, client2.State, "Client not disconnected");
            }
            finally
            {
                client1.Dispose();
                client2.Dispose();
            }
        }

        protected abstract INatterClient GetClient1();
        protected abstract INatterClient GetClient2();

        protected abstract IAddress GetClient1Address();
        protected abstract IAddress GetClient2Address();

    }
}
