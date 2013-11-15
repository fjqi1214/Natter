using System;
using Natter.Messaging;

namespace Natter.Connecting.States
{
    internal class StateManager
    {
        private readonly IConnectionActions _actions;
        private IStateManager _currentState;
        private readonly object _lockObject = new object();

        public ConnectionState State
        {
            get
            {
                lock (_lockObject)
                {
                    return _currentState.State;
                }
            }
        }

        public StateManager(IConnectionActions actions)
        {
            _actions = actions;
            _currentState = new DisconnectedStateManager(_actions);
        }

        public void Call()
        {
            lock (_lockObject)
            {
                TryUpdateState(new CallingStateManager(_actions));
            }
        }

        public void End()
        {
            lock (_lockObject)
            {
                TryUpdateState(new DisconnectedStateManager(_actions));
                _actions.EndCall();
            }
        }

        public void ProcessMessage(MessageType type, IMessage message)
        {
            lock (_lockObject)
            {
                var state = _currentState.ProcessMessage(type, message);
                TryUpdateState(state);
            }
        }

        public void Send(IField[] data)
        {
            lock (_lockObject)
            {
                _currentState.Send(data);
            }
        }

        public void Ping()
        {
            lock (_lockObject)
            {
                _currentState.Ping();
            }
        }

        private void TryUpdateState(IStateManager state)
        {
            if (state != _currentState)
            {
                _currentState = state;
                _currentState.Start();
            }
        }
    }
}
