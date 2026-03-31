using Moq;
using Opc.Ua.Client;
using Rebelit.OT.Discover.EdgeApp.Connections.OPCUA.Clients;

namespace Rebelit.OT.Discover.EdgeApp.Connections.OPCUA.Tests.Clients;

[TestFixture]
public class SessionStateChangedEventArgsTests
{
    [Test]
    public void Constructor_WithStateAndSession_SetsPropertiesCorrectly()
    {
        var session = new Mock<ISession>(MockBehavior.Strict).Object;

        var args = new SessionStateChangedEventArgs(UAClientState.Connected, session);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(args.State, Is.EqualTo(UAClientState.Connected));
            Assert.That(args.Session, Is.SameAs(session));
        }
    }

    [Test]
    public void Constructor_WithStateOnly_SetsSessionToNull()
    {
        var args = new SessionStateChangedEventArgs(UAClientState.Disconnected);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(args.State, Is.EqualTo(UAClientState.Disconnected));
            Assert.That(args.Session, Is.Null);
        }
    }

    [Test]
    [TestCase(UAClientState.Connected)]
    [TestCase(UAClientState.Reconnecting)]
    [TestCase(UAClientState.Reconnected)]
    [TestCase(UAClientState.Disconnected)]
    public void Constructor_AllStateValues_PreservesState(UAClientState state)
    {
        var args = new SessionStateChangedEventArgs(state);

        Assert.That(args.State, Is.EqualTo(state));
    }
}
