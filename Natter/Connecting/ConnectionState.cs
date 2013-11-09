using System.Linq;

namespace Natter.Connecting
{
    public enum ConnectionState
    {
        None,
        Listening,
        Calling,
        Connected,
        Disconnected
    }

    public class ConnectionStateType
    {
        public static readonly ConnectionStateType None = new ConnectionStateType(ConnectionState.None);
        public static readonly ConnectionStateType Listening = new ConnectionStateType(ConnectionState.Listening);
        public static readonly ConnectionStateType Calling = new ConnectionStateType(ConnectionState.Calling);
        public static readonly ConnectionStateType Connected = new ConnectionStateType(ConnectionState.Connected);
        public static readonly ConnectionStateType Disconnected = new ConnectionStateType(ConnectionState.Disconnected);

        public static readonly ConnectionStateType[] All = new[] { None, Listening, Calling, Connected, Disconnected };

        public ConnectionState State
        {
            get;
            private set;
        }

        private ConnectionStateType(ConnectionState state)
        {
            State = state;
        }

        public static ConnectionStateType Parse(ConnectionState state)
        {
            return All.FirstOrDefault(s => s.State == state);
        }
    }
}
