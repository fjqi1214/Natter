using Natter.Transporting;

namespace Natter.Client
{
    public class UdpNatterClient : NatterClient
    {
        public UdpNatterClient(string host, int port)
            : base(new UdpTransport(new UdpAddress(host, port)))
        {
        }

        public void Call(string host, int port)
        {
            Call(new UdpAddress(host, port));
        }
    }
}
