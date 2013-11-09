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
    public class MemoryConnectionTest : ConnectionTest
    {
        private const string Client1Address = "Client1";
        private const string Client2Address = "Client2";

        protected override INatterClient GetClient1()
        {
            return new MemoryNatterClient(Client1Address);
        }

        protected override INatterClient GetClient2()
        {
            return new MemoryNatterClient(Client2Address);
        }

        protected override IAddress GetClient1Address()
        {
            return new MemoryAddress(Client1Address);
        }

        protected override IAddress GetClient2Address()
        {
            return new MemoryAddress(Client2Address);
        }
    }
}
