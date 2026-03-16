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
            ApplicationId = "app-id",
            BearerToken = "token",
            CompanyId = "company-id",
            BaseUrl = "https://custom.example.com",
            Version = 3,
        };

        Assert.Multiple(() =>
        {
            Assert.That(config.ApplicationId, Is.EqualTo("app-id"));
            Assert.That(config.BearerToken, Is.EqualTo("token"));
            Assert.That(config.CompanyId, Is.EqualTo("company-id"));
            Assert.That(config.BaseUrl, Is.EqualTo("https://custom.example.com"));
            Assert.That(config.Version, Is.EqualTo(3));
        });
    }
}
