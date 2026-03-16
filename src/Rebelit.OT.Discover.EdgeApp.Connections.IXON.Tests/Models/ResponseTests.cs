using System.Text.Json;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;

namespace Rebelit.OT.Discover.EdgeApp.Connections.IXON.Tests.Models;

[TestFixture]
public class ResponseTests
{
    [Test]
    public void AllProperties_WhenAssigned_ReturnAssignedValues()
    {
        var response = new Response<int>
        {
            Data = 42,
            MoreAfter = "cursor",
            Status = "ok",
            Type = "list",
        };

        Assert.Multiple(() =>
        {
            Assert.That(response.Data, Is.EqualTo(42));
            Assert.That(response.MoreAfter, Is.EqualTo("cursor"));
            Assert.That(response.Status, Is.EqualTo("ok"));
            Assert.That(response.Type, Is.EqualTo("list"));
        });
    }

    [Test]
    public void Deserialize_WithCompleteJson_MapsAllProperties()
    {
        const string json = """{"data":"hello","moreAfter":"next","status":"200","type":"item"}""";

        var result = JsonSerializer.Deserialize<Response<string>>(json)!;

        Assert.Multiple(() =>
        {
            Assert.That(result.Data, Is.EqualTo("hello"));
            Assert.That(result.MoreAfter, Is.EqualTo("next"));
            Assert.That(result.Status, Is.EqualTo("200"));
            Assert.That(result.Type, Is.EqualTo("item"));
        });
    }
}
