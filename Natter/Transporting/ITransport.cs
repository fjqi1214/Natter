using System;
using Natter.Messaging;

namespace Natter.Transporting
{
    public interface ITransport : IDisposable
    {
        void Send(string connectionId, IMessage message, IAddress address);
        void Listen(Action<IMessage, IAddress> handleMessage);
    }
}
