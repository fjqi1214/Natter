using System;
using Natter.Messaging;

namespace Natter.Connecting.States
{
    internal class NoneStateManager : IStateManager
    {
        public ConnectionState State
        {
            get { return ConnectionState.None; }
        }

        public void Start()
        {
        }

        public IStateManager ProcessMessage(MessageType type, IMessage message)
        {
            throw new Exception("Cannot process a message yet");
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
