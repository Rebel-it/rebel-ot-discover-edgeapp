using System.Text.Json;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;

namespace Rebelit.OT.Discover.EdgeApp.Connections.IXON.Tests.Models;

[TestFixture]
public class SourceTests
{
    [Test]
    public void PublicId_WhenAssigned_ReturnsAssignedValue()
    {
        var source = new Source { PublicId = "src-1" };

        Assert.That(source.PublicId, Is.EqualTo("src-1"));
    }

    [Test]
    public void Deserialize_WithPublicIdJson_MapsPublicId()
    {
        const string json = """{"publicId":"src-abc"}""";

        var result = JsonSerializer.Deserialize<Source>(json)!;

        Assert.That(result.PublicId, Is.EqualTo("src-abc"));
    }
}
