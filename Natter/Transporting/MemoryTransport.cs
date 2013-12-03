using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Natter.Byte;
using Natter.Messaging;

namespace Natter.Transporting
{
    public class MemoryTransport : ITransport
    {
        private static readonly Dictionary<string, MemoryTransport> Transports = new Dictionary<string, MemoryTransport>();
        private readonly Dictionary<string, MemoryAddress> _addresses = new Dictionary<string, MemoryAddress>();
        private readonly MemoryAddress _me;

        private Action<IMessage, IAddress> _handleMessage;

        public MemoryTransport(string me)
        {
            _me = new MemoryAddress(me);
            Transports[me] = this;
        }

        public void Send(string connectionId, IMessage message, IAddress address)
        {
            var destination = address as MemoryAddress;
            if (destination == null)
            {
                throw new Exception("The destination is invalid");
            }
            Send(message, destination.Address);
        }

        public void Listen(Action<IMessage, IAddress> handleMessage)
        {
            _handleMessage = handleMessage;
        }

        private void Receive(IMessage message, string address)
        {
            if (_handleMessage != null)
            {
                var connectionId = message.ConnectionId.GetString();
                if (!string.IsNullOrEmpty(connectionId))
                {
                    MemoryAddress from;
                    if (!_addresses.TryGetValue(connectionId, out from))
                    {
                        from = new MemoryAddress(address);
                        _addresses[connectionId] = from;
                    }
                    _handleMessage(message, from);
                }
            }
        }

        private void Send(IMessage message, string them)
        {
            MemoryTransport t;
            if (Transports.TryGetValue(them, out t))
            {
                var send = new Task(() => t.Receive(message, _me.Address));
                send.Start();
            }
        }

        public void Dispose()
        {
            Transports.Clear();
        }
    }
}
