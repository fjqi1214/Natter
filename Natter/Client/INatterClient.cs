using System;
using Natter.Connecting;
using Natter.Messaging;
using Natter.Transporting;

namespace Natter.Client
{
    public interface INatterClient : IDisposable
    {
        INatterClient OnConnected(Action<INatterConnection> onConnected);
        INatterClient OnDisconnected(Action<INatterConnection> onDisconnected);
        INatterClient OnError(Action<INatterConnection, Exception> onError);
        INatterClient OnData(Action<INatterConnection, FieldData> onData);

        INatterConnection Call(IAddress address);
    }
}
