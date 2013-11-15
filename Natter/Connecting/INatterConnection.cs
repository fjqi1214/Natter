using Natter.Messaging;

namespace Natter.Connecting
{
    public interface INatterConnection
    {
        string ConnectionId { get; }
        ConnectionState State { get; }

        void Send(IField[] data);
        void Close();
    }
}
