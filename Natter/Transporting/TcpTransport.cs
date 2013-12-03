using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using Natter.Byte;
using Natter.Messaging;

namespace Natter.Transporting
{
    public class TcpTransport : ITransport
    {
        private readonly int _port;
        private readonly Dictionary<string, TcpTalk> _talkers = new Dictionary<string, TcpTalk>();
        private readonly TcpListen _listener;
        private Action<IMessage, IAddress> _handleMessage;

        public TcpTransport(int port)
        {
            _port = port;
            _listener = new TcpListen(_port, NewClientConnection);
        }

        public void Send(string connectionId, IMessage message, IAddress destination)
        {
            var talker = GetTalker(connectionId, (TcpAddress)destination);
            Send(talker, message.Serialise());
        }

        private TcpTalk GetTalker(string connectionId, TcpAddress destination)
        {
            lock (_talkers)
            {
                TcpTalk talker;
                if (!_talkers.TryGetValue(connectionId, out talker))
                {
                    talker = new TcpTalk(_port, destination);
                    talker.HandleMessage(HandleMessage);
                    _talkers[connectionId] = talker;
                }
                return talker;
            }
        }

        private void HandleMessage(byte[] data, TcpTalk talker)
        {
            if (_handleMessage != null)
            {
                var message = Message.Deserialise(data);
                var connectionId = message.ConnectionId.GetString();
                if (!string.IsNullOrEmpty(connectionId))
                {
                    if (!_talkers.ContainsKey(connectionId))
                    {
                        _talkers[connectionId] = talker;
                    }
                    _handleMessage(message, talker.Destination);
                }
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

        public void Listen(Action<IMessage, IAddress> handleMessage)
        {
            _handleMessage = handleMessage;
        }

        private void NewClientConnection(TcpClient client)
        {
            lock (_talkers)
            {
                var talker = new TcpTalk(client);
                talker.HandleMessage(HandleMessage);
            }
        }

        public void Dispose()
        {
            _listener.Close();
            foreach (var talker in _talkers.Values)
            {
                talker.Close();
            }
        }
    }
}
