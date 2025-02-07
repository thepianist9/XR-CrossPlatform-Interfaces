using System;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// Base class representing a connection state.
    /// </summary>
    abstract class XRConnectionState
    {
        protected XROptionalConnectionManager m_ConnectionManager;

        public abstract void Enter();

        public abstract void Exit();

        public virtual void OnClientConnected(ulong clientId) { }
        
        public virtual void OnClientDisconnect(ulong clientId) { }

        public virtual void OnServerStarted() { }

        public virtual void StartClientIP(string ipaddress, ushort port) { }
        
        public virtual void StartHostIP(string ipaddress, ushort port) { }

        public virtual void OnUserRequestedShutdown() { }

        public virtual void OnTransportFailure() { }
    }
}
