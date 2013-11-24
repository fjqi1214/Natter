using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Natter.Transporting
{
    public class TcpTalk
    {
        private readonly TcpClient _client;
        private bool _alive = true;
        private readonly byte[] _buffer = new byte[8 * 1024];
        private int _bufferPosition;

        private Action<byte[]> _handleMessage;

        private const int LengthBufferSize = sizeof(int);

        public TcpTalk(string host, int port)
        {
            _client = new TcpClient();
            _client.Connect(host, port);
            ReadFromStream();
        }

        public TcpTalk(TcpClient client)
        {
            _client = client;
            ReadFromStream();
        }

        public void SendData(byte[] data)
        {
            var lengthPrefix = BitConverter.GetBytes(data.Length);

            var message = new byte[lengthPrefix.Length + data.Length];
            lengthPrefix.CopyTo(message, 0);
            data.CopyTo(message, lengthPrefix.Length);

            _client.GetStream().WriteAsync(message, 0, message.Length);
        }

        public void HandleMessage(Action<byte[]> handleMessage)
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
                        _handleMessage(data);
                    }
                }
            }
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
