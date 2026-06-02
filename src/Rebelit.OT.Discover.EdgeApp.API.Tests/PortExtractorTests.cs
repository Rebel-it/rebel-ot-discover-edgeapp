using Rebelit.OT.Discover.EdgeApp.API.Utilities;

namespace Rebelit.OT.Discover.EdgeApp.Tests;

[TestFixture]
public class PortExtractorTests
{
    [Test]
    public void ExtractPort_WithValidOpcUaUrlWithCustomPort_ReturnsPort()
    {
        var result = PortExtractor.ExtractPort("opc.tcp://172.27.21.3:4840");

        Assert.That(result, Is.EqualTo(4840));
    }

    [Test]
    public void ExtractPort_WithValidOpcUaUrlWithoutPort_ReturnsNull()
    {
        var result = PortExtractor.ExtractPort("opc.tcp://172.27.21.3");

        Assert.That(result, Is.Null);
    }

    [Test]
    public void ExtractPort_WithInvalidUrl_ReturnsNull()
    {
        var result = PortExtractor.ExtractPort("not-a-uri");

        Assert.That(result, Is.Null);
    }

    [Test]
    public void ExtractPort_WithHttpDefaultPort_ReturnsNull()
    {
        var result = PortExtractor.ExtractPort("http://localhost:80");

        Assert.That(result, Is.Null);
    }

    [Test]
    public void ExtractPort_WithHttpsCustomPort_ReturnsPort()
    {
        var result = PortExtractor.ExtractPort("https://localhost:5001");

        Assert.That(result, Is.EqualTo(5001));
    }
}
