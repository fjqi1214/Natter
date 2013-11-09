using System;
using Natter.Messaging;

namespace Natter.Connecting.States
{
    internal class ListeningStateManager : IStateManager
    {
        private readonly IConnectionActions _actions;

        public ConnectionState State
        {
            get { return ConnectionState.Listening; }
        }

        public ListeningStateManager(IConnectionActions actions)
        {
            _actions = actions;
        }

        public void Start()
        {
        }

        public IStateManager ProcessMessage(MessageType type, IMessage message)
        {
            if (type == MessageType.Start)
            {
                _actions.AnswerCall(message);
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
        }
    }
}
