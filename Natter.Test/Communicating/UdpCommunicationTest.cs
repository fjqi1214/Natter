using System;
using NUnit.Framework;
using Natter.Client;
using Natter.Transporting;

namespace Natter.Test.Communicating
{
    [TestFixture]
    public class UdpCommunicationTest : CommunicationTest
    {
        private const string Localhost = "localhost";
        private const int Client1Port = 16000;
        private const int Client2Port = 16001;

        protected override INatterClient GetClient1()
        {
            return new UdpNatterClient(Client1Port);
        }

        protected override INatterClient GetClient2()
        {
            return new UdpNatterClient(Client2Port);
        }

        protected override IAddress GetClient2Address()
        {
            return new UdpAddress(Localhost, Client2Port);
        }
    }
}
