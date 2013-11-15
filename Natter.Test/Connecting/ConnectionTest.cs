using System;
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
                var connection = client.Call(GetClient1Address());

                Assert.AreEqual(ConnectionState.Calling, connection.State, "Client not calling");
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
                INatterConnection connection1 = null;
                client1.OnConnected(c => { reset1.Set(); connection1 = c; });
                var connection2 = client2.OnConnected(c => reset2.Set()).Call(GetClient1Address());

                Assert.IsTrue(reset1.WaitOne(TimeSpan.FromSeconds(200)), "Failed to connect");
                Assert.IsTrue(reset2.WaitOne(TimeSpan.FromSeconds(200)), "Failed to connect");
                Assert.IsNotNull(connection1, "Connection is null");
                Assert.IsNotNull(connection2, "Connection is null");
                Assert.AreEqual(ConnectionState.Connected, connection1.State, "Client not connected");
                Assert.AreEqual(ConnectionState.Connected, connection2.State, "Client not connected");
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
                // Start connecting before listener is ready
                INatterConnection connection1 = null;
                var connection2 = client2.OnConnected(c => reset2.Set()).Call(GetClient1Address());
                client1.OnConnected(c => { reset1.Set(); connection1 = c; });

                Assert.IsTrue(reset1.WaitOne(TimeSpan.FromSeconds(200)), "Failed to connect");
                Assert.IsTrue(reset2.WaitOne(TimeSpan.FromSeconds(200)), "Failed to connect");
                Assert.IsNotNull(connection1, "Connection is null");
                Assert.IsNotNull(connection2, "Connection is null");
                Assert.AreEqual(ConnectionState.Connected, connection1.State, "Client not connected");
                Assert.AreEqual(ConnectionState.Connected, connection2.State, "Client not connected");
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
                INatterConnection connection1 = null;
                client1.OnConnected(c => { reset1.Set(); connection1 = c; });
                var connection2 = client2.OnConnected(c => reset2.Set()).Call(GetClient1Address());

                Assert.IsTrue(reset1.WaitOne(TimeSpan.FromSeconds(2)), "Failed to connect");
                Assert.IsTrue(reset2.WaitOne(TimeSpan.FromSeconds(2)), "Failed to connect");

                reset1.Reset();
                reset2.Reset();
                client1.OnDisconnected(c => reset1.Set());
                client2.OnDisconnected(c => reset2.Set());
                connection1.Close();

                Assert.IsTrue(reset1.WaitOne(TimeSpan.FromSeconds(2)), "Failed to disconnect");
                Assert.IsTrue(reset2.WaitOne(TimeSpan.FromSeconds(2)), "Failed to disconnect");
                Assert.AreEqual(ConnectionState.Disconnected, connection1.State, "Client not disconnected");
                Assert.AreEqual(ConnectionState.Disconnected, connection2.State, "Client not disconnected");
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
                client1.OnConnected(c => reset1.Set());
                var connection2 = client2.OnConnected(c => reset2.Set()).Call(GetClient1Address());

                Assert.IsTrue(reset1.WaitOne(TimeSpan.FromSeconds(2)), "Failed to connect");
                Assert.IsTrue(reset2.WaitOne(TimeSpan.FromSeconds(2)), "Failed to connect");

                reset1.Reset();
                reset2.Reset();
                client2.OnError((c, e) => reset1.Set()).OnDisconnected(c => reset2.Set());
                client1.Dispose();

                Assert.IsTrue(reset1.WaitOne(TimeSpan.FromSeconds(50)), "There was no error");
                Assert.IsTrue(reset2.WaitOne(TimeSpan.FromSeconds(50)), "Should be disconnected");
                Assert.AreEqual(ConnectionState.Disconnected, connection2.State, "Client not disconnected");
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
