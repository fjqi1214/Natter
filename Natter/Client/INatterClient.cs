using System;
using Natter.Connecting;
using Natter.Messaging;
using Natter.Transporting;

namespace Natter.Client
{
    public interface INatterClient : IDisposable
    {
        ConnectionState State { get; }

        INatterClient OnConnected(Action onConnected);
        INatterClient OnDisconnected(Action onDisconnected);
        INatterClient OnError(Action<Exception> onError);
        INatterClient OnData(Action<IField[]> onData);

        void Call(IAddress address);
        void Listen();
        void Send(IField[] data);
        void End();
    }
}
