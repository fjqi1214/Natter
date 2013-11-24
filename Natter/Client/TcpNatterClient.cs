using Natter.Transporting;

namespace Natter.Client
{
    public class TcpNatterClient : NatterClient
    {
        public TcpNatterClient(string host, int port)
            : base(new TcpTransport(new TcpAddress(host, port)))
        {
        }

        public void Call(string host, int port)
        {
            Call(new TcpAddress(host, port));
        }
    }
}
