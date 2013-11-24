using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Natter.Transporting
{
    public class TcpListen
    {
        private bool _alive = true;
        private TcpListener _listener;
        private readonly int _port;
        private readonly Action<TcpClient> _newConnection;

        public TcpListen(int port, Action<TcpClient> newConnection)
        {
            _port = port;
            _newConnection = newConnection;
            StartListening();
        }

        private void StartListening()
        {
            _listener = new TcpListener(IPAddress.Any, _port);
            _listener.Start();
            WaitForConnection();
        }

        private void WaitForConnection()
        {
            _listener.AcceptTcpClientAsync().ContinueWith(AcceptListenSocket);
        }

        private void AcceptListenSocket(Task<TcpClient> task)
        {
            lock (_listener)
            {
                if (_alive)
                {
                    var client = task.Result;
                    if (_newConnection != null)
                    {
                        _newConnection(client);
                    }
                    WaitForConnection();
                }
            }
        }

        public void Close()
        {
            lock (_listener)
            {
                _alive = false;
                _listener.Stop();
            }
        }
    }
}
