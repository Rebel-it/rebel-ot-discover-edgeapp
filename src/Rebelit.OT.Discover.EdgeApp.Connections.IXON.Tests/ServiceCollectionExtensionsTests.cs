using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Agents;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;

namespace Rebelit.OT.Discover.EdgeApp.Connections.IXON.Tests;

[TestFixture]
public class ServiceCollectionExtensionsTests
{
    private static IConfiguration BuildConfig(
        string appId = "app", string companyId = "company", string token = "token",
        string email = "email@test.com", string password = "password")
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["IXON_ApplicationId"] = appId,
                ["IXON_CompanyId"] = companyId,
                ["IXON_BearerToken"] = token,
                ["OPCUA_Username"] = email,
                ["OPCUA_Password"] = password,
            })
            .Build();
    }

    [Test]
    public void AddIXONClient_RegistersIApiClientAsSingleton()
    {
        var services = new ServiceCollection();

        services.AddIXONClient(BuildConfig());

        var descriptor = services.Single(d => d.ServiceType == typeof(IApiClient));
        Assert.That(descriptor.Lifetime, Is.EqualTo(ServiceLifetime.Singleton));
    }

    [Test]
    public void AddIXONClient_WithValidParameters_ConfiguresOptionsCorrectly()
    {
        var services = new ServiceCollection();

        services.AddIXONClient(BuildConfig("my-app", "my-company", "my-token", "test@example.com", "pass"));

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

        var result = services.AddIXONClient(BuildConfig("a", "b", "c"));

        Assert.That(result, Is.SameAs(services));
    }
}
