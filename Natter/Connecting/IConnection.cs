using System;
using Natter.Messaging;

namespace Natter.Connecting
{
    public interface IConnection
    {
        ConnectionState State { get; }

        IConnection OnConnected(Action onConnected);
        IConnection OnDisconnected(Action onDisconnected);
        IConnection OnError(Action<Exception> onError);
        IConnection OnData(Action<IField[]> onData);
    }
}
