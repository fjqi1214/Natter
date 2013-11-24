using NUnit.Framework;
using Natter.Client;
using Natter.Transporting;

namespace Natter.Test.Communicating
{
    [TestFixture]
    public class TcpCommunicationTest : CommunicationTest
    {
        private const string Localhost = "localhost";
        private const int Client1Port = 16000;
        private const int Client2Port = 16001;

        protected override INatterClient GetClient1()
        {
            return new TcpNatterClient(Localhost, Client1Port);
        }

        protected override INatterClient GetClient2()
        {
            return new TcpNatterClient(Localhost, Client2Port);
        }

        protected override IAddress GetClient2Address()
        {
            return new TcpAddress(Localhost, Client2Port);
        }
    }
}
