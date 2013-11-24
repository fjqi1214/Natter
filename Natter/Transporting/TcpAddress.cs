namespace Natter.Transporting
{
    public class TcpAddress : IAddress
    {
        private readonly string _serialised;

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

        public TcpAddress(string host, int port)
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
            var tokens = address.Split(new [] { ':' });
            return new TcpAddress(tokens[0], int.Parse(tokens[1]));
        }
    }
}
