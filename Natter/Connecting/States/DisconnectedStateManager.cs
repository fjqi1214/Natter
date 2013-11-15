using System;
using Natter.Messaging;

namespace Natter.Connecting.States
{
    internal class DisconnectedStateManager : IStateManager
    {
        private readonly IConnectionActions _actions;
        
        public ConnectionState State
        {
            get { return ConnectionState.Disconnected; }
        }

        public DisconnectedStateManager(IConnectionActions actions)
        {
            _actions = actions;
        }

        public void Start()
        {
            _actions.OnDisconnected();
        }

        public IStateManager ProcessMessage(MessageType type, IMessage message)
        {
            if (type == MessageType.Start)
            {
                _actions.AnswerCall(message);
                return new ConnectedStateManager(_actions);
            }
            return this;
        }

        public void Send(IField[] data)
        {
            throw new Exception("Cannot send. Please connect.");
        }

        public void Ping()
        {
        }
    }
}
