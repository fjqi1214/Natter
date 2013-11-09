using System;
using System.Net;
using Natter.Byte;
using Natter.Messaging;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Natter.Transporting
{
    public class UdpTransport : ITransport
    {
        private bool _alive = true;
        private readonly UdpAddress _address;
        private readonly UdpClient _sendClient;
        private readonly UdpClient _receiveClient;
        private Action<IMessage> _handleMessage;

        public UdpTransport(UdpAddress address)
        {
            _address = address;
            _sendClient = new UdpClient();
            _sendClient.DontFragment = true;
            _receiveClient = new UdpClient(_address.Port);
            _receiveClient.DontFragment = true;
        }

        public void Send(IAddress address, IMessage message)
        {
            Send((UdpAddress)address, message.Serialise());
        }

        private void Send(UdpAddress address, byte[] data)
        {
            var send = new Task(() =>
                {
                    lock (_sendClient)
                    {
                        var res = _sendClient.Send(data, data.Length, address.Host, address.Port);
                        if (res != data.Length)
                        {
                            throw new Exception("Did not send the entire message");
                        }
                    }
                });
            send.Start();
        }

        public void Listen(Action<IMessage> handleMessage)
        {
            StartListening(handleMessage);
        }

        private void HandleMessage(IMessage message)
        {
            if (_handleMessage != null)
            {
                _handleMessage(message);
            }
        }

        private void StartListening(Action<IMessage> handleMessage)
        {
            _handleMessage = handleMessage;
            var endpoint = new IPEndPoint(IPAddress.Any, _address.Port);
            ReceiveData(endpoint);
        }

        private void ReceiveData(IPEndPoint endpoint)
        {
            if (_alive)
            {
                _receiveClient.BeginReceive(DataReceived, endpoint);
            }
        }

        private void DataReceived(IAsyncResult res)
        {
            if (_alive)
            {
                var endPoint = (IPEndPoint) res.AsyncState;
                var data = _receiveClient.EndReceive(res, ref endPoint);
                if (data.Length > 0)
                {
                    HandleMessage(Message.Deserialise(data));
                }
                ReceiveData(endPoint);
            }
        }

        public IAddress GetAddress()
        {
            return _address;
        }

        public IAddress DeserialiseAddress(string address)
        {
            return _address.Deserialise(address);
        }

        public void Dispose()
        {
            _alive = false;
            _sendClient.Close();
            _receiveClient.Close();
        }
    }
}
