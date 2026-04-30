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
    public void AddIXONClient_WhenCalled_ReturnsTheSameServiceCollection()
    {
        var services = new ServiceCollection();

        var result = services.AddIXONClient(BuildConfig("a", "b", "c"));

        Assert.That(result, Is.SameAs(services));
    }
}
