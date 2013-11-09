using System;
using Natter.Connecting;
using Natter.Messaging;
using Natter.Transporting;

namespace Natter.Client
{
    public abstract class NatterClient : INatterClient
    {
        private bool _disposed;
        private readonly ITransport _transport;
        private readonly Connection _connection;

        private Action _onConnected;
        private Action _onDisconnected;
        private Action<Exception> _onError;
        private Action<IField[]> _onData;

        public ConnectionState State
        {
            get { return _connection.State; }
        }

        protected NatterClient(ITransport transport)
        {
            _transport = transport;
            _connection = new Connection(_transport);
            _connection.OnConnected(OnConnected).
                        OnDisconnected(OnDisconnected).
                        OnError(OnError).
                        OnData(OnData);
        }

        private void OnConnected()
        {
            if (_onConnected != null)
            {
                _onConnected();
            }
        }

        public INatterClient OnConnected(Action onConnected)
        {
            _onConnected = onConnected;
            return this;
        }

        private void OnDisconnected()
        {
            if (_onDisconnected != null)
            {
                _onDisconnected();
            }
        }

        public INatterClient OnDisconnected(Action onDisconnected)
        {
            _onDisconnected = onDisconnected;
            return this;
        }

        private void OnData(IField[] data)
        {
            if (_onData != null)
            {
                _onData(data);
            }
        }

        public INatterClient OnData(Action<IField[]> onData)
        {
            _onData = onData;
            return this;
        }

        private void OnError(Exception error)
        {
            if (_onError != null)
            {
                _onError(error);
            }
        }

        public INatterClient OnError(Action<Exception> onError)
        {
            _onError = onError;
            return this;
        }

        public void Call(IAddress address)
        {
            _connection.Call(address);
        }

        public void Listen()
        {
            _connection.Listen();
        }

        public void Send(IField[] data)
        {
            _connection.Send(data);
        }

        public void End()
        {
            _connection.End();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                }
                _connection.Dispose();
                _transport.Dispose();
            }
            _disposed = true;
        }

        ~NatterClient()
        {
            Dispose(false);
        }
    }
}
