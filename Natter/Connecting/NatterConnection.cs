using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Natter.Byte;
using Natter.Messaging;
using Natter.Transporting;
using Natter.Connecting.States;

namespace Natter.Connecting
{
    internal class NatterConnection : INatterConnection, IConnectionActions, IDisposable
    {
        private readonly ITransport _transport;
        private readonly StateManager _stateManager;
        private readonly byte[] _sourceAddress;
        private IAddress _destinationAddress;
        private readonly Timer _timer;
        private bool _disposed;

        private Action<INatterConnection> _onConnected;
        private Action<INatterConnection> _onDisconnected;
        private Action<INatterConnection, Exception> _onError;
        private Action<INatterConnection, IField[]> _onData;

        private const int PingPeriod = 1000;

        private static readonly ConnectionState[] StartStates = new[] { ConnectionState.None, ConnectionState.Disconnected };

        public ConnectionState State
        {
            get { return _stateManager.State; }
        }

        public string ConnectionId
        {
            get;
            private set;
        }

        public byte[] ConnectionIdRaw
        {
            get;
            private set;
        }

        public NatterConnection(ITransport transport, string connectionId)
        {
            _transport = transport;
            ConnectionId = connectionId;
            ConnectionIdRaw = connectionId.GetBytes();
            _stateManager = new StateManager(this);
            _sourceAddress = _transport.GetAddress().Serialise().GetBytes();
            _timer = new Timer(o => Ping(), null, PingPeriod, PingPeriod);
        }

        public void Call(IAddress address)
        {
            if (!StartStates.Contains(State))
            {
                throw new Exception("Invalid start state");
            }
            _destinationAddress = address;
            _stateManager.Call();
        }

        public void HandleMessage(MessageType messageType, IMessage message)
        {
            _stateManager.ProcessMessage(messageType, message);
        }

        public void Send(IField[] data)
        {
            _stateManager.Send(data);
        }

        public void Close()
        {
            _stateManager.End();
        }

        public void StartCall(byte[] transactionId)
        {
            try
            {
                var message = MessageType.CreateStartMessage(ConnectionIdRaw, transactionId, _sourceAddress);

                SendMessage(message, _destinationAddress);
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }

        public void AnswerCall(IMessage originalMessage)
        {
            try
            {
                _destinationAddress = _transport.DeserialiseAddress(originalMessage.From.GetString());
                var message = MessageType.CreateOkMessage(ConnectionIdRaw, originalMessage.TransactionId, _sourceAddress);
                SendMessage(message, _destinationAddress);
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }

        public void EndCall()
        {
            try
            {
                _transport.Listen(null);
                var message = MessageType.CreateEndMessage(ConnectionIdRaw, CreateNewId(), _sourceAddress);
                SendMessage(message, _destinationAddress);
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }

        private void SendMessage(IMessage message, IAddress address)
        {
            _transport.Send(address, message);
        }

        public void SendData(IField[] data)
        {
            try
            {
                var message = MessageType.CreateDataMessage(ConnectionIdRaw, CreateNewId(), _sourceAddress, data);
                SendMessage(message, _destinationAddress);
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }

        private void Ping()
        {
            try
            {
                _stateManager.Ping();
            }
            catch (Exception ex)
            {
                Close();
                OnError(ex);
            }
        }

        public void SendPing(byte[] transactionId)
        {
            var message = MessageType.CreatePingMessage(ConnectionIdRaw, transactionId, _sourceAddress);
            SendMessage(message, _destinationAddress);
        }

        public void SendOk(byte[] transactionId)
        {
            var message = MessageType.CreateOkMessage(ConnectionIdRaw, transactionId, _sourceAddress);
            SendMessage(message, _destinationAddress);
        }

        public void OnConnected()
        {
            if (_onConnected != null)
            {
                var onConnected = new Task(() => _onConnected(this));
                onConnected.Start();
            }
        }

        public NatterConnection OnConnected(Action<INatterConnection> onConnected)
        {
            _onConnected = onConnected;
            return this;
        }

        public void OnDisconnected()
        {
            if (_onDisconnected != null)
            {
                var onDisconnected = new Task(() => _onDisconnected(this));
                onDisconnected.Start();
            }
        }

        public NatterConnection OnDisconnected(Action<INatterConnection> onDisconnected)
        {
            _onDisconnected = onDisconnected;
            return this;
        }

        public void OnError(Exception ex)
        {
            if (_onError != null)
            {
                var onError = new Task(() => _onError(this, ex));
                onError.Start();
            }
        }

        public NatterConnection OnError(Action<INatterConnection, Exception> onError)
        {
            _onError = onError;
            return this;
        }

        public void OnData(IField[] data)
        {
            if (_onData != null)
            {
                var onData = new Task(() => _onData(this, data));
                onData.Start();
            }
        }

        public NatterConnection OnData(Action<INatterConnection, IField[]> onData)
        {
            _onData = onData;
            return this;
        }

        public static byte[] CreateNewId()
        {
            return Guid.NewGuid().ToString().GetBytes();
        }

        public void Dispose()
        {
            DisposeInternal();
            GC.SuppressFinalize(this);
        }

        public void DisposeInternal()
        {
            if (!_disposed)
            {
                _timer.Dispose();
            }
            _disposed = true;
        }

        ~NatterConnection()
        {
            Dispose();
        }
    }
}