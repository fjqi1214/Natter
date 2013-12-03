using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Natter.Transporting
{
    public class TcpTalk
    {
        private readonly TcpClient _client;
        private bool _alive = true;
        private int _bufferPosition;
        private readonly byte[] _buffer = new byte[8 * 1024];

        private Action<byte[], TcpTalk> _handleMessage;

        private const int LengthBufferSize = sizeof(int);

        public TcpAddress Destination
        {
            get;
            private set;
        }

        public TcpTalk(int port, TcpAddress destination)
        {
            Destination = destination;

            var ipLocalEndPoint = new IPEndPoint(GetIpAddress(), port);
            _client = new TcpClient(ipLocalEndPoint);
            _client.LingerState.Enabled = false;
            _client.Connect(destination.Host, destination.Port);
            ReadFromStream();
        }

        public TcpTalk(TcpClient client)
        {
            var ipAddress = ((IPEndPoint)client.Client.RemoteEndPoint).Address;
            var port = ((IPEndPoint)client.Client.RemoteEndPoint).Port;
            Destination = new TcpAddress(ipAddress, port);
            _client = client;
            _client.LingerState.Enabled = false;
            ReadFromStream();
        }

        public void SendData(byte[] data)
        {
            if (_alive)
            {
                var lengthPrefix = BitConverter.GetBytes(data.Length);

                var message = new byte[lengthPrefix.Length + data.Length];
                lengthPrefix.CopyTo(message, 0);
                data.CopyTo(message, lengthPrefix.Length);

                _client.GetStream().WriteAsync(message, 0, message.Length);
            }
        }

        public void HandleMessage(Action<byte[], TcpTalk> handleMessage)
        {
            _handleMessage = handleMessage;
        }

        private void ReadFromStream()
        {
            var size = _buffer.Length - _bufferPosition;
            _client.GetStream().ReadAsync(_buffer, _bufferPosition, size).ContinueWith(DataReceived);
        }

        private void DataReceived(Task<int> result)
        {
            lock (_client)
            {
                if (_alive)
                {
                    var bytesAdded = result.Result;
                    _bufferPosition += bytesAdded;
                    TryProcessBuffer();
                    ReadFromStream();
                }
            }
        }

        private void TryProcessBuffer()
        {
            if (_bufferPosition >= LengthBufferSize)
            {
                var lengthBuffer = new byte[LengthBufferSize];
                Array.Copy(_buffer, 0, lengthBuffer, 0, LengthBufferSize);
                var expectedLength = BitConverter.ToInt32(lengthBuffer, 0);

                if (_bufferPosition >= LengthBufferSize + expectedLength)
                {
                    var data = new byte[expectedLength];
                    Array.Copy(_buffer, LengthBufferSize, data, 0, expectedLength);
                    Array.Copy(_buffer, _bufferPosition, _buffer, 0, _buffer.Length - _bufferPosition);
                    _bufferPosition -= (expectedLength + LengthBufferSize);

                    if (_handleMessage != null)
                    {
                        _handleMessage(data, this);
                    }
                }
            }
        }

        private IPAddress GetIpAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip;
                }
            }
            return null;
        }

        public void Close()
        {
            lock (_client)
            {
                _alive = false;
                _client.Close();
            }
        }
    }
}
