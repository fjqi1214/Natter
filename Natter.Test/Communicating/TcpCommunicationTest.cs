using NUnit.Framework;
using Natter.Client;
using Natter.Transporting;

namespace Natter.Test.Communicating
{
    [TestFixture]
    public class TcpCommunicationTest : CommunicationTest
    {
        private const int Client1Port = 17012;
        private const int Client2Port = 17010;

        protected override INatterClient GetClient1()
        {
            return new TcpNatterClient(Client1Port);
        }

        protected override INatterClient GetClient2()
        {
            return new TcpNatterClient(Client2Port);
        }

        protected override IAddress GetClient2Address()
        {
            return new TcpAddress(TcpAddress.GetLocalIpAddress(), Client2Port);
        }
    }
}
