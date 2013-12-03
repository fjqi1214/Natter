using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Natter.Transporting
{
    public class TcpAddress : IAddress
    {
        private readonly string _serialised;

        public IPAddress Host
        {
            get;
            private set;
        }

        public int Port
        {
            get;
            private set;
        }

        public TcpAddress(IPAddress host, int port)
        {
            Host = host;
            Port = port;
            _serialised = string.Format("{0}:{1}", Host, Port);
        }

        public string Serialise()
        {
            return _serialised;
        }

        public IAddress Deserialise(string address)
        {
            return null;
        }

        public static IPAddress GetLocalIpAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            var endpoint = host.AddressList.FirstOrDefault(e => e.AddressFamily == AddressFamily.InterNetwork);
            return endpoint;
        }
    }
}
