using System;
using Natter.Byte;
using Natter.Messaging;

namespace Natter.Connecting.States
{
    internal class CallingStateManager : IStateManager
    {
        private readonly IConnectionActions _actions;
        private readonly byte[] _transactionId;
        private int _tryCount;

        public ConnectionState State
        {
            get { return ConnectionState.Calling; }
        }

        public CallingStateManager(IConnectionActions actions)
        {
            _actions = actions;
            _transactionId = NatterConnection.CreateNewId();
        }

        public void Start()
        {
            TryCall();
        }

        public IStateManager ProcessMessage(MessageType type, IMessage message)
        {
            if (type == MessageType.Ok && ByteTools.Compare(_transactionId, message.TransactionId))
            {
                return new ConnectedStateManager(_actions);
            }
            if (type == MessageType.End)
            {
                return new DisconnectedStateManager(_actions);
            }
            return this;
        }

        public void Send(IField[] data)
        {
            throw new Exception("Cannot send. Please connect.");
        }

        public void Ping()
        {
            TryCall();
        }

        private void TryCall()
        {
            _tryCount++;
            if (_tryCount > 3)
            {
                //throw new Exception("Timed out trying to call");
            }
            if (_tryCount == 1)
            {
                _actions.StartCall(_transactionId);
            }
        }
    }
}
