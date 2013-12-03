using Natter.Transporting;

namespace Natter.Client
{
    public class UdpNatterClient : NatterClient
    {
        public UdpNatterClient(int port)
            : base(new UdpTransport(port))
        {
        }

        public void Call(string host, int port)
        {
            Call(new UdpAddress(host, port));
        }
    }
}
