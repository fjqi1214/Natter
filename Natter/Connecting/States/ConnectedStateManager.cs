using System;
using Natter.Byte;
using Natter.Messaging;

namespace Natter.Connecting.States
{
    internal class ConnectedStateManager : IStateManager
    {
        private readonly IConnectionActions _actions;
        private byte[] _pingTransactionId;
        private int _pingTryCount;

        public ConnectionState State
        {
            get { return ConnectionState.Connected; }
        }

        public ConnectedStateManager(IConnectionActions actions)
        {
            _actions = actions;
        }

        public void Start()
        {
            _actions.OnConnected();
        }

        public IStateManager ProcessMessage(MessageType type, IMessage message)
        {
            if (type == MessageType.Data)
            {
                _pingTryCount = 0;
                _actions.OnData(message.Data);
                return this;
            }
            if (type == MessageType.End)
            {
                return new DisconnectedStateManager(_actions);
            }
            if (type == MessageType.Ping)
            {
                _pingTryCount = 0;
                _actions.SendOk(message.TransactionId);
            }
            if (_pingTryCount > 0 && type == MessageType.Ok &&
                ByteTools.Compare(_pingTransactionId, message.TransactionId))
            {
                _pingTryCount = 0;
            }
            return this;
        }

        public void Send(IField[] data)
        {
            _actions.SendData(data);
        }

        public void Ping()
        {
            _pingTryCount++;
            if (_pingTryCount > 5)
            {
                throw new Exception("Ping timed out. Connection lost.");
            }
            if (_pingTryCount >=2)
            {
                if (_pingTryCount == 2)
                {
                    _pingTransactionId = NatterConnection.CreateNewId();
                }
                _actions.SendPing(_pingTransactionId);
            }
        }
    }
}
