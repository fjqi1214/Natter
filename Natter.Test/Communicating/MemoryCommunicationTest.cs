using NUnit.Framework;
using Natter.Client;
using Natter.Transporting;

namespace Natter.Test.Communicating
{
    [TestFixture]
    public class MemoryCommunicationTest : CommunicationTest
    {
        protected override INatterClient GetClient1()
        {
            return new MemoryNatterClient("Client1");
        }

        protected override INatterClient GetClient2()
        {
            return new MemoryNatterClient("Client2");
        }

        protected override IAddress GetClient2Address()
        {
            return new MemoryAddress("Client2");
        }
    }
}
