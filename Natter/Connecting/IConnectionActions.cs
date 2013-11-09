using Natter.Messaging;

namespace Natter.Connecting
{
    internal interface IConnectionActions
    {
        void StartCall(byte[] transactionId);
        void AnswerCall(IMessage message);
        void EndCall();
        void OnConnected();
        void OnDisconnected();
        void OnData(IField[] transactionId);
        void SendData(IField[] transactionId);
        void SendPing(byte[] transactionId);
        void SendOk(byte[] transactionId);
    }
}
