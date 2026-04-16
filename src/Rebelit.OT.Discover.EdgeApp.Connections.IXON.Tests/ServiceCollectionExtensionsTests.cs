using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Agents;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;

namespace Rebelit.OT.Discover.EdgeApp.Connections.IXON.Tests;

[TestFixture]
public class ServiceCollectionExtensionsTests
{
    [Test]
    public void AddIXONClient_RegistersIApiClientAsSingleton()
    {
        var services = new ServiceCollection();

        services.AddIXONClient("app", "company", "token", "email@test.com", "password");

        var descriptor = services.Single(d => d.ServiceType == typeof(IApiClient));
        Assert.That(descriptor.Lifetime, Is.EqualTo(ServiceLifetime.Singleton));
    }

    [Test]
    public void AddIXONClient_WithValidParameters_ConfiguresOptionsCorrectly()
    {
        var services = new ServiceCollection();

        services.AddIXONClient("my-app", "my-company", "my-token", "test@example.com", "pass");

        var options = services.BuildServiceProvider().GetRequiredService<IOptions<Configuration>>();
        Assert.Multiple(() =>
        {
            Assert.That(options.Value.ApplicationId, Is.EqualTo("my-app"));
            Assert.That(options.Value.CompanyId, Is.EqualTo("my-company"));
            Assert.That(options.Value.BearerToken, Is.EqualTo("my-token"));
        });
    }

    [Test]
    public void AddIXONClient_WhenCalled_ReturnsTheSameServiceCollection()
    {
        var services = new ServiceCollection();

        var result = services.AddIXONClient("a", "b", "c", "email@test.com", "password");

        Assert.That(result, Is.SameAs(services));
    }
}
