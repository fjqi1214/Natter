using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Natter.Messaging;

namespace Natter.Transporting
{
    public class TcpTransport : ITransport
    {
        private bool _alive = true;
        private readonly TcpAddress _address;
        private readonly Dictionary<string, TcpTalk> _talkers = new Dictionary<string, TcpTalk>();
        private TcpListen _listener;
        private Action<IMessage> _handleMessage;

        public TcpTransport(TcpAddress address)
        {
            _address = address;
            _listener = new TcpListen(_address.Port, NewClientConnection);
        }

        public void Send(IAddress address, IMessage message)
        {
            var tcpAddress = (TcpAddress)address;
            var talker = GetTalker(tcpAddress);
            Send(talker, message.Serialise());
        }

        private TcpTalk GetTalker(TcpAddress address)
        {
            lock (_talkers)
            {
                TcpTalk talker;
                var key = address.Serialise();
                if (!_talkers.TryGetValue(key, out talker))
                {
                    talker = new TcpTalk(new TcpClient(address.Host, address.Port));
                    _talkers[key] = talker;
                    talker.HandleMessage(HandleMessage);
                }
                return talker;
            }
        }

        private void HandleMessage(byte[] data)
        {
            if (_handleMessage != null)
            {
                var message = Message.Deserialise(data);
                _handleMessage(message);
            }
        }

        private void Send(TcpTalk talker, byte[] data)
        {
            var send = new Task(() =>
            {
                lock (talker)
                {
                    talker.SendData(data);
                }
            });
            send.Start();
        }

        public void Listen(Action<IMessage> handleMessage)
        {
            _handleMessage = handleMessage;
        }

        private void NewClientConnection(TcpClient client)
        {
            lock (_talkers)
            {
                var host = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
                var port = ((IPEndPoint)client.Client.RemoteEndPoint).Port;
                var address = new TcpAddress(host, port);
                var talker = new TcpTalk(client);
                talker.HandleMessage(HandleMessage);

                _talkers[address.Serialise()] = talker;
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
            _listener.Close();
            foreach (var talker in _talkers.Values)
            {
                talker.Close();
            }
        }
    }
}
