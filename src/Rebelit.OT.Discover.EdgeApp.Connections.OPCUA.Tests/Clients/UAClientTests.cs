using Opc.Ua;
using Rebelit.OT.Discover.EdgeApp.Connections.OPCUA.Clients;
using Rebelit.OT.Discover.EdgeApp.Connections.OPCUA.Telemetry;

namespace Rebelit.OT.Discover.EdgeApp.Connections.OPCUA.Tests.Clients;

[TestFixture]
public class UAClientTests
{
    private UAClient _sut = null!;
    private bool _sutDisposed;

    [SetUp]
    public void SetUp()
    {
        _sutDisposed = false;
        var telemetry = new ConsoleTelemetry();
        _sut = new UAClient(
            CreateMinimalApplicationConfiguration(telemetry),
            telemetry,
            validateResponse: (_, _) => { }
        );
    }

    [TearDown]
    public void TearDown()
    {
        if (!_sutDisposed)
        {
            _sut.Dispose();
        }
    }

    [Test]
    public void ConnectTimeout_Default_Is30000()
    {
        Assert.That(_sut.ConnectTimeout, Is.EqualTo(30_000));
    }

    [Test]
    public void ReverseConnectTimeout_Default_Is30000()
    {
        Assert.That(_sut.ReverseConnectTimeout, Is.EqualTo(30_000));
    }

    [Test]
    public void IsConnected_WhenNoSession_ReturnsFalse()
    {
        Assert.That(_sut.IsConnected, Is.False);
    }

    [Test]
    public async Task ConnectAsync_WhenDisposed_ThrowsObjectDisposedException()
    {
        _sut.Dispose();
        _sutDisposed = true;

        Assert.ThrowsAsync<ObjectDisposedException>(() =>
            _sut.ConnectAsync("opc.tcp://localhost:4840")
        );

        await Task.CompletedTask;
    }

    [Test]
    public void ConnectAsync_WhenServerUrlIsNull_ThrowsArgumentNullException()
    {
        Assert.ThrowsAsync<ArgumentNullException>(() => _sut.ConnectAsync(null!));
    }

    [Test]
    public async Task DisconnectAsync_WhenNoSession_DoesNotThrow()
    {
        Assert.DoesNotThrowAsync(() => _sut.DisconnectAsync());

        await Task.CompletedTask;
    }

    [Test]
    public void SessionStateChanged_WhenNoHandlers_RaisesNoException()
    {
        Assert.DoesNotThrow(() =>
        {
            _sut.Dispose();
            _sutDisposed = true;
        });
    }

    [Test]
    public void SessionStateChanged_WhenHandlerRegistered_IsNotFiredOnConstruction()
    {
        int eventCount = 0;
        _sut.SessionStateChanged += (_, _) => eventCount++;

        Assert.That(eventCount, Is.EqualTo(0));
    }

    private static ApplicationConfiguration CreateMinimalApplicationConfiguration(
        ITelemetryContext telemetry
    ) =>
        new()
        {
            ApplicationName = "UAClientTests",
            ApplicationType = ApplicationType.Client,
            CertificateValidator = new CertificateValidator(telemetry),
            TransportConfigurations = [],
            ClientConfiguration = new ClientConfiguration(),
            TransportQuotas = new TransportQuotas(),
        };
}
