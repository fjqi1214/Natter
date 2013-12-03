using System.Net;
using Natter.Transporting;

namespace Natter.Client
{
    public class TcpNatterClient : NatterClient
    {
        public TcpNatterClient(int port)
            : base(new TcpTransport(port))
        {
        }

        public void Call(string host, int port)
        {
            Call(new TcpAddress(IPAddress.Parse(host), port));
        }
    }
}
