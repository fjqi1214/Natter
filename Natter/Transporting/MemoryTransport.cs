using System;
using System.Threading.Tasks;
using Natter.Messaging;
using System.Collections.Generic;

namespace Natter.Transporting
{
    public class MemoryTransport : ITransport
    {
        private static readonly Dictionary<string, MemoryTransport> _transports = new Dictionary<string, MemoryTransport>();
        private readonly MemoryAddress _me;

        private Action<IMessage> _handleMessage;

        public MemoryTransport(string me)
        {
            _me = new MemoryAddress(me);
            _transports[me] = this;
        }

        public void Send(IAddress address, IMessage message)
        {
            var destination = address as MemoryAddress;
            if (destination == null)
            {
                throw new Exception("The destination is invalid");
            }
            Send(message, destination.Address);
        }

        public void Listen(Action<IMessage> handleMessage)
        {
            _handleMessage = handleMessage;
        }

        public IAddress GetAddress()
        {
            return _me;
        }

        public IAddress DeserialiseAddress(string address)
        {
            return new MemoryAddress(address);
        }

        private void Receive(IMessage message)
        {
            if (_handleMessage != null)
            {
                _handleMessage(message);
            }
        }

        private static void Send(IMessage message, string them)
        {
            MemoryTransport t;
            if (_transports.TryGetValue(them, out t))
            {
                var send = new Task(() => t.Receive(message));
                send.Start();
            }
        }

        public void Dispose()
        {
            _transports.Clear();
        }

        public void Send(ITransport address, IMessage message)
        {
            throw new NotImplementedException();
        }
    }
}
