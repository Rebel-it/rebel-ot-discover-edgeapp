using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;

namespace Rebelit.OT.Discover.EdgeApp.Connections.IXON.Tests.Models;

[TestFixture]
public class ConfigurationTests
{
    [Test]
    public void BaseUrlAndVersion_ByDefault_HaveExpectedValues()
    {
        var config = new Configuration();

        Assert.Multiple(() =>
        {
            Assert.That(config.BaseUrl, Is.EqualTo("https://portal.ixon.cloud"));
            Assert.That(config.Version, Is.EqualTo(2));
        });
    }

    [Test]
    public void AllProperties_WhenAssigned_ReturnAssignedValues()
    {
        var config = new Configuration
        {
            BaseUrl = new Uri("https://custom.example.com"),
            Version = 3,
        };

        Assert.Multiple(() =>
        {
            Assert.That(config.BaseUrl, Is.EqualTo(new Uri("https://custom.example.com")));
            Assert.That(config.Version, Is.EqualTo(3));
        });
    }
}
