using Natter.Messaging;

namespace Natter.Connecting.States
{
    internal interface IStateManager
    {
        ConnectionState State { get; }
        void Start();
        IStateManager ProcessMessage(MessageType type, IMessage message);
        void Send(IField[] data);
        void Ping();
    }
}
