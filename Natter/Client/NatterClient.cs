using System;
using System.Collections.Generic;
using Natter.Byte;
using Natter.Connecting;
using Natter.Messaging;
using Natter.Transporting;

namespace Natter.Client
{
    public abstract class NatterClient : INatterClient
    {
        private bool _disposed;
        private readonly ITransport _transport;
        private readonly Dictionary<string, NatterConnection> _connections;

        private Action<INatterConnection> _onConnected;
        private Action<INatterConnection> _onDisconnected;
        private Action<INatterConnection, Exception> _onError;
        private Action<INatterConnection, FieldData> _onData;

        protected NatterClient(ITransport transport)
        {
            _connections = new Dictionary<string, NatterConnection>();
            _transport = transport;
            _transport.Listen(HandleMessage);
        }

        public INatterConnection Call(IAddress address)
        {
            var connection = CreateNewConnection(CreateConnectionId());
            _connections[connection.ConnectionId] = connection;
            connection.Call(address);
            return connection;
        }

        private void HandleMessage(IMessage message)
        {
            NatterConnection connection = null;
            try
            {
                var connectionId = message.ConnectionId.GetString();
                var messageType = MessageType.Parse(message.MessageType);
                connection = TryGetConnection(connectionId);
                if (connection != null)
                {
                    connection.HandleMessage(messageType, message);
                }
                else if (messageType == MessageType.Start)
                {
                    connection = CreateNewConnection(connectionId);
                    connection.HandleMessage(messageType, message);
                }
            }
            catch (Exception ex)
            {
                if (connection != null)
                {
                    OnError(connection, ex);
                }
            }
        }

        private NatterConnection CreateNewConnection(string connectionId)
        {
            lock (_connections)
            {
                var connection = new NatterConnection(_transport, connectionId);
                connection.OnConnected(OnConnected).
                           OnDisconnected(OnDisconnected).
                           OnData(OnData).
                           OnError(OnError);

                _connections[connection.ConnectionId] = connection;
                return connection;
            }
        }

        private NatterConnection TryGetConnection(string connectionId)
        {
            lock (_connections)
            {
                NatterConnection connection = null;
                if (_connections.TryGetValue(connectionId, out connection))
                {
                    return connection;
                }
            }
            return null;
        }

        private string CreateConnectionId()
        {
            return Guid.NewGuid().ToString();
        }

        private void OnConnected(INatterConnection connection)
        {
            if (_onConnected != null)
            {
                _onConnected(connection);
            }
        }

        public INatterClient OnConnected(Action<INatterConnection> onConnected)
        {
            _onConnected = onConnected;
            return this;
        }

        private void OnDisconnected(INatterConnection connection)
        {
            if (_onDisconnected != null)
            {
                _onDisconnected(connection);
            }
        }

        public INatterClient OnDisconnected(Action<INatterConnection> onDisconnected)
        {
            _onDisconnected = onDisconnected;
            return this;
        }

        private void OnData(INatterConnection connection, FieldData data)
        {
            if (_onData != null)
            {
                _onData(connection, data);
            }
        }

        public INatterClient OnData(Action<INatterConnection, FieldData> onData)
        {
            _onData = onData;
            return this;
        }

        private void OnError(INatterConnection connection, Exception error)
        {
            if (_onError != null)
            {
                _onError(connection, error);
            }
        }

        public INatterClient OnError(Action<INatterConnection, Exception> onError)
        {
            _onError = onError;
            return this;
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
                _transport.Dispose();
                foreach (var connection in _connections.Values)
                {
                    connection.Dispose();
                }
            }
            _disposed = true;
        }

        ~NatterClient()
        {
            Dispose();
        }
    }
}
