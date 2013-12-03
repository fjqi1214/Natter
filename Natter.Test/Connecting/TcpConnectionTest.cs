using NUnit.Framework;
using Natter.Client;
using Natter.Transporting;

namespace Natter.Test.Connecting
{
    [TestFixture]
    public class TcpConnectionTest : ConnectionTest
    {
        private const int Client1Port = 16000;
        private const int Client2Port = 16001;

        protected override INatterClient GetClient1()
        {
            return new TcpNatterClient(Client1Port);
        }

        protected override INatterClient GetClient2()
        {
            return new TcpNatterClient(Client2Port);
        }

        protected override IAddress GetClient1Address()
        {
            return new TcpAddress(TcpAddress.GetLocalIpAddress(), Client1Port);
        }

        protected override IAddress GetClient2Address()
        {
            return new TcpAddress(TcpAddress.GetLocalIpAddress(), Client2Port);
        }
    }
}