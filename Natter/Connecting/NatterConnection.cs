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
        private readonly IAddress _destination;
        private readonly StateManager _stateManager;
        private readonly Timer _timer;
        private bool _disposed;

        private Action<INatterConnection> _onConnected;
        private Action<INatterConnection> _onDisconnected;
        private Action<INatterConnection, Exception> _onError;
        private Action<INatterConnection, FieldData> _onData;

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

        public NatterConnection(string connectionId, ITransport transport, IAddress destination)
        {
            _transport = transport;
            _destination = destination;
            ConnectionId = connectionId;
            ConnectionIdRaw = connectionId.GetBytes();
            _stateManager = new StateManager(this);
            _timer = new Timer(o => Ping(), null, PingPeriod, PingPeriod);
        }

        public void Call()
        {
            if (!StartStates.Contains(State))
            {
                throw new Exception("Invalid start state");
            }
            _stateManager.Call();
        }

        public void HandleMessage(MessageType messageType, IMessage message)
        {
            _stateManager.ProcessMessage(messageType, message);
        }

        public void Send(FieldData data)
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
                var message = MessageType.CreateStartMessage(ConnectionIdRaw, transactionId);

                SendMessage(message);
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
                var message = MessageType.CreateOkMessage(ConnectionIdRaw, originalMessage.TransactionId);
                SendMessage(message);
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
                var message = MessageType.CreateEndMessage(ConnectionIdRaw, CreateNewId());
                SendMessage(message);
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }

        private void SendMessage(IMessage message)
        {
            _transport.Send(ConnectionId, message, _destination);
        }

        public void SendData(FieldData data)
        {
            try
            {
                var message = MessageType.CreateDataMessage(ConnectionIdRaw, CreateNewId(), data.GetFields());
                SendMessage(message);
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
            var message = MessageType.CreatePingMessage(ConnectionIdRaw, transactionId);
            SendMessage(message);
        }

        public void SendOk(byte[] transactionId)
        {
            var message = MessageType.CreateOkMessage(ConnectionIdRaw, transactionId);
            SendMessage(message);
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

        public void OnData(FieldData data)
        {
            if (_onData != null)
            {
                var onData = new Task(() => _onData(this, data));
                onData.Start();
            }
        }

        public NatterConnection OnData(Action<INatterConnection, FieldData> onData)
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