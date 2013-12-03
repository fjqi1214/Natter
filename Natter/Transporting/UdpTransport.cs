using System;
using System.Collections.Generic;
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
        private readonly int _port;
        private readonly UdpClient _client;
        private Action<IMessage, IAddress> _handleMessage;
        private readonly Dictionary<string, UdpAddress> _addresses = new Dictionary<string, UdpAddress>();

        public UdpTransport(int port)
        {
            _port = port;
            _client = new UdpClient(_port) { DontFragment = true };
        }

        public void Send(string connectionId, IMessage message, IAddress address)
        {
            var destination = address as UdpAddress;
            if (destination == null)
            {
                throw new Exception("The destination is invalid");
            }
            Send(destination, message.Serialise());
        }

        private void Send(UdpAddress address, byte[] data)
        {
            var send = new Task(() =>
                {
                    lock (_client)
                    {
                        var res = _client.Send(data, data.Length, address.Host, address.Port);
                        if (res != data.Length)
                        {
                            throw new Exception("Did not send the entire message");
                        }
                    }
                });
            send.Start();
        }

        public void Listen(Action<IMessage, IAddress> handleMessage)
        {
            StartListening(handleMessage);
        }

        private void HandleMessage(IMessage message, IAddress address)
        {
            if (_handleMessage != null)
            {
                _handleMessage(message, address);
            }
        }

        private void StartListening(Action<IMessage, IAddress> handleMessage)
        {
            _handleMessage = handleMessage;
            var endpoint = new IPEndPoint(IPAddress.Any, _port);
            ReceiveData(endpoint);
        }

        private void ReceiveData(IPEndPoint endpoint)
        {
            if (_alive)
            {
                _client.BeginReceive(DataReceived, endpoint);
            }
        }

        private void DataReceived(IAsyncResult res)
        {
            if (_alive)
            {
                var endPoint = (IPEndPoint)res.AsyncState;
                try
                {
                    var data = _client.EndReceive(res, ref endPoint);
                    if (data.Length > 0)
                    {
                        var message = Message.Deserialise(data);

                        var connectionId = message.ConnectionId.GetString();
                        if (!string.IsNullOrEmpty(connectionId))
                        {
                            UdpAddress from;
                            if (!_addresses.TryGetValue(connectionId, out from))
                            {
                                from = new UdpAddress(endPoint.Address.ToString(), endPoint.Port);
                                _addresses[connectionId] = from;
                            }
                            HandleMessage(message, from);
                        }
                    }
                }
                catch
                { // Needs to stay alive
                }
                ReceiveData(endPoint);
            }
        }

        public void Dispose()
        {
            _alive = false;
            _client.Close();
        }
    }
}
