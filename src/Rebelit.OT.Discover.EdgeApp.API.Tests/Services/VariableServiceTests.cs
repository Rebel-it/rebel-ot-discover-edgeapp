using Microsoft.Extensions.Logging;
using Rebelit.OT.Discover.EdgeApp.API.Services;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;
using Rebelit.OT.Discover.EdgeApp.Tests.Services.Helpers;

namespace Rebelit.OT.Discover.EdgeApp.Tests.Services;

[TestFixture]
public class VariableServiceTests
{
    [Test]
    public async Task GetVariablesAsync_WhenApiReturnsVariables_ReturnsVariables()
    {
        Variable[] expected =
        [
            new() { PublicId = "v-1", Address = "ns=2;s=Temp", Name = "Temperature", Slug = "temperature", Type = "float", Source = new Source { PublicId = "src-1" } },
            new() { PublicId = "v-2", Address = "ns=2;s=Pressure", Name = "Pressure", Slug = "pressure", Type = "float", Source = new Source { PublicId = "src-1" } },
        ];

        var apiClient = new ApiClientSpy { DataVariables = expected };
        var unitTestHelper = new UnitTestHelpers();
        var authContext = unitTestHelper.CreateAuthenticationContext("agent-123", "src-1");
        var logger = new TestLogger<VariableService>(isInfoEnabled: true);
        var sut = new VariableService(apiClient, authContext, logger);

        var result = await sut.GetVariablesAsync();

        Assert.Multiple(() =>
        {
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result.Select(v => v.PublicId), Is.EqualTo(new[] { "v-1", "v-2" }));
        });
    }

    [Test]
    public async Task GetVariablesAsync_WhenApiReturnsNullData_ReturnsEmptyList()
    {
        var apiClient = new ApiClientSpy { DataVariables = null };
        var unitTestHelper = new UnitTestHelpers();
        var authContext = unitTestHelper.CreateAuthenticationContext("agent-123");
        var logger = new TestLogger<VariableService>(isInfoEnabled: true);
        var sut = new VariableService(apiClient, authContext, logger);

        var result = await sut.GetVariablesAsync();

        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task GetVariablesAsync_WhenInformationLoggingEnabled_LogsRetrievedCountAndAgentId()
    {
        Variable[] variables =
        [
            new() { PublicId = "v-1", Address = "ns=2;s=Temp", Name = "Temperature", Slug = "temperature", Type = "float", Source = new Source { PublicId = "src-1" } },
            new() { PublicId = "v-2", Address = "ns=2;s=Pressure", Name = "Pressure", Slug = "pressure", Type = "float", Source = new Source { PublicId = "src-1" } },
        ];

        var apiClient = new ApiClientSpy { DataVariables = variables };
        var unitTestHelper = new UnitTestHelpers();
        var authContext = unitTestHelper.CreateAuthenticationContext("agent-42", "src-1");
        var logger = new TestLogger<VariableService>(isInfoEnabled: true);
        var sut = new VariableService(apiClient, authContext, logger);

        await sut.GetVariablesAsync();

        var infoLog = logger.Entries.Single(e => e.LogLevel == LogLevel.Information);
        Assert.That(infoLog.Message, Is.EqualTo("Retrieved 2 variables for agent agent-42."));
    }
}
