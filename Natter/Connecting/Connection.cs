using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Natter.Byte;
using Natter.Connecting.States;
using Natter.Messaging;
using Natter.Transporting;

namespace Natter.Connecting
{
    internal class Connection : IConnection, IConnectionActions, IDisposable
    {
        private readonly ITransport _transport;
        private byte[] _connectionId = new byte[0];
        private readonly StateManager _stateManager;
        private readonly byte[] _sourceAddress;
        private IAddress _destinationAddress;
        private readonly Timer _timer;

        private Action _onConnected;
        private Action _onDisconnected;
        private Action<Exception> _onError;
        private Action<IField[]> _onData;

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

        public Connection(ITransport transport)
        {
            _transport = transport;
            ConnectionId = Guid.NewGuid().ToString();
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

        public void Listen()
        {
            if (!StartStates.Contains(State))
            {
                throw new Exception("Invalid start state");
            }
            _transport.Listen(HandleMessage);
            _stateManager.Listen();
        }

        public void Send(IField[] data)
        {
            _stateManager.Send(data);
        }

        public void End()
        {
            _stateManager.End();
        }

        public void StartCall(byte[] transactionId)
        {
            try
            {
                _transport.Listen(HandleMessage);

                _connectionId = ConnectionId.GetBytes();
                var message = MessageType.CreateStartMessage(_connectionId, transactionId, _sourceAddress);

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
                _connectionId = originalMessage.ConnectionId;

                var message = MessageType.CreateOkMessage(_connectionId, originalMessage.TransactionId, _sourceAddress);
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
                var message = MessageType.CreateEndMessage(_connectionId, CreateNewId(), _sourceAddress);
                SendMessage(message, _destinationAddress);
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }

        private void HandleMessage(IMessage message)
        {
            try
            {
                if (true) //_connectionId.Length == 0 || ByteTools.Compare(message.ConnectionId, _connectionId))
                {
                    var messageType = MessageType.Parse(message.MessageType);
                    _stateManager.ProcessMessage(messageType, message);
                }
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
                var message = MessageType.CreateDataMessage(_connectionId, CreateNewId(), _sourceAddress, data);
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
                End();
                OnError(ex);
            }
        }

        public void SendPing(byte[] transactionId)
        {
            var message = MessageType.CreatePingMessage(_connectionId, transactionId, _sourceAddress);
            SendMessage(message, _destinationAddress);
        }

        public void SendOk(byte[] transactionId)
        {
            var message = MessageType.CreateOkMessage(_connectionId, transactionId, _sourceAddress);
            SendMessage(message, _destinationAddress);
        }

        public void OnConnected()
        {
            if (_onConnected != null)
            {
                _onConnected();
            }
        }

        public IConnection OnConnected(Action onConnected)
        {
            _onConnected = onConnected;
            return this;
        }

        public void OnDisconnected()
        {
            if (_onDisconnected != null)
            {
                _onDisconnected();
            }
        }

        public IConnection OnDisconnected(Action onDisconnected)
        {
            _onDisconnected = onDisconnected;
            return this;
        }

        public void OnError(Exception ex)
        {
            if (_onError != null)
            {
                var onError = new Task(() => _onError(ex));
                onError.Start();
            }
        }

        public IConnection OnError(Action<Exception> onError)
        {
            _onError = onError;
            return this;
        }

        public void OnData(IField[] data)
        {
            if (_onData != null)
            {
                var onData = new Task(() => _onData(data));
                onData.Start();
            }
        }

        public IConnection OnData(Action<IField[]> onData)
        {
            _onData = onData;
            return this;
        }

        public void Dispose()
        {
            _timer.Dispose();
        }

        public static byte[] CreateNewId()
        {
            return Guid.NewGuid().ToString().GetBytes();
        }
    }
}