using Opc.Ua.Client;

namespace Rebelit.OT.Discover.EdgeApp.Connections.OPCUA.Clients;

public sealed class SessionStateChangedEventArgs : EventArgs
{
    public UAClientState State { get; }
    public ISession? Session { get; }

    public SessionStateChangedEventArgs(UAClientState state, ISession? session = null)
    {
        State = state;
        Session = session;
    }
}
