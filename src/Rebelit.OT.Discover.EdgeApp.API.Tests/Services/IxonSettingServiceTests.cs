using Rebelit.OT.Discover.EdgeApp.API.Services;
using Rebelit.OT.Discover.EdgeApp.Tests.Services.Helpers;

namespace Rebelit.OT.Discover.EdgeApp.Tests.Services;

[TestFixture]
public class IxonSettingServiceTests
{
    [Test]
    public async Task PushDeviceConfigAsync_WhenApiReturnsStatus_ReturnsStatusAndUsesAgentIdFromContext()
    {
        // Arrange
        var apiClient = new ApiClientSpy { PushConfigurationStatus = "success" };
        var authContext = UnitTestHelpers.CreateAuthenticationContext("agent-42");
        var sut = new IxonSettingService(apiClient, authContext);

        // Act
        var result = await sut.PushDeviceConfigAsync();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo("success"));
            Assert.That(apiClient.PushConfigurationCallCount, Is.EqualTo(1));
            Assert.That(apiClient.PushConfigurationAgentId, Is.EqualTo("agent-42"));
        });
    }

    [Test]
    public async Task PushDeviceConfigAsync_WhenApiReturnsNull_ReturnsNull()
    {
        // Arrange
        var apiClient = new ApiClientSpy { ReturnNullFromPushConfiguration = true };
        var authContext = UnitTestHelpers.CreateAuthenticationContext("agent-42");
        var sut = new IxonSettingService(apiClient, authContext);

        // Act
        var result = await sut.PushDeviceConfigAsync();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Null);
            Assert.That(apiClient.PushConfigurationCallCount, Is.EqualTo(1));
            Assert.That(apiClient.PushConfigurationAgentId, Is.EqualTo("agent-42"));
        });
    }

    [Test]
    public void PushDeviceConfigAsync_WhenAgentIdIsMissing_ThrowsInvalidOperationException()
    {
        // Arrange
        var apiClient = new ApiClientSpy();
        var authContext = UnitTestHelpers.CreateAuthenticationContext(agentId: string.Empty);
        var sut = new IxonSettingService(apiClient, authContext);

        // Act / Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => await sut.PushDeviceConfigAsync());

        Assert.Multiple(() =>
        {
            Assert.That(ex!.Message, Is.EqualTo("AgentId is required but was not set."));
            Assert.That(apiClient.PushConfigurationCallCount, Is.EqualTo(0));
        });

    }
}
