using System;
using Natter.Messaging;

namespace Natter.Transporting
{
    public interface ITransport : IDisposable
    {
        void Send(IAddress address, IMessage message);
        void Listen(Action<IMessage> handleMessage);

        IAddress GetAddress();
        IAddress DeserialiseAddress(string address);
    }
}
