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
        void OnData(FieldData data);
        void SendData(FieldData data);
        void SendPing(byte[] transactionId);
        void SendOk(byte[] transactionId);
    }
}
