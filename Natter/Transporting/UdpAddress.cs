namespace Natter.Transporting
{
    public class UdpAddress : IAddress
    {
        public string Host
        {
            get;
            private set;
        }

        public int Port
        {
            get;
            private set;
        }

        public UdpAddress(string host, int port)
        {
            Host = host;
            Port = port;
        }

        public string Serialise()
        {
            return string.Format("{0}:{1}", Host, Port);
        }

        public IAddress Deserialise(string address)
        {
            string[] tokens = address.Split(new [] { ':' });
            return new UdpAddress(tokens[0], int.Parse(tokens[1]));
        }
    }
}
