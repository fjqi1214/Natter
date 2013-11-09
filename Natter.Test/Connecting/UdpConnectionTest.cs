using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Natter.Client;
using Natter.Transporting;

namespace Natter.Test.Connecting
{
    [TestFixture]
    public class UdpConnectionTest : ConnectionTest
    {
        private const string Localhost = "localhost";
        private const int Client1Port = 16000;
        private const int Client2Port = 16001;

        protected override INatterClient GetClient1()
        {
            return new UdpNatterClient(Localhost, Client1Port);
        }

        protected override INatterClient GetClient2()
        {
            return new UdpNatterClient(Localhost, Client2Port);
        }

        protected override IAddress GetClient1Address()
        {
            return new UdpAddress(Localhost, Client1Port);
        }

        protected override IAddress GetClient2Address()
        {
            return new UdpAddress(Localhost, Client2Port);
        }
    }
}
