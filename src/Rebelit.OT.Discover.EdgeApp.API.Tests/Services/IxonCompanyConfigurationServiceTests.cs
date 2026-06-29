using Rebelit.OT.Discover.EdgeApp.API.Services;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;
using Rebelit.OT.Discover.EdgeApp.Connections.SecureEdgePro;
using Rebelit.OT.Discover.EdgeApp.SharedKernel;
using Rebelit.OT.Discover.EdgeApp.Tests.Services.Helpers;

namespace Rebelit.OT.Discover.EdgeApp.Tests.Services;

[TestFixture]
public class IxonCompanyConfigurationServiceTests
{
    [Test]
    public async Task GetConfigurationAsync_WhenAllDataIsAvailable_ReturnsCompanyAndAgentId()
    {
        // Arrange
        var apiClient = new ApiClientSpy
        {
            AssociatedCompanies = [new Company { PublicId = "comp-1" }],
            Agents = [new Agent { PublicId = "agent-1", DeviceId = "SN-123456" }],
        };
        var secureEdgeApiClient = new SecureEdgeApiClientSpy
        {
            SystemInfoResult = new Result<SecureEdgeSystemInfo>
            {
                Data = new SecureEdgeSystemInfo { SerialNumber = "SN-123456" },
            },
        };
        var authContext = UnitTestHelpers.CreateAuthenticationContext("agent-any");
        var sut = new IxonCompanyConfigurationService(apiClient, authContext, secureEdgeApiClient);

        // Act
        var result = await sut.GetConfigurationAsync();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.ErrorMessage, Is.Null);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.CompanyId, Is.EqualTo("comp-1"));
            Assert.That(result.Data.AgentId, Is.EqualTo("agent-1"));
            Assert.That(authContext.IxonHeaders.CompanyId, Is.EqualTo("comp-1"));

            Assert.That(apiClient.GetAssociatedCompanyCallCount, Is.EqualTo(1));
            Assert.That(secureEdgeApiClient.GetSystemInfoCallCount, Is.EqualTo(1));
            Assert.That(apiClient.GetAgentsCallCount, Is.EqualTo(1));
        });
    }

    [Test]
    public async Task GetConfigurationAsync_WhenNoAssociatedCompany_ReturnsErrorAndStopsEarly()
    {
        // Arrange
        var apiClient = new ApiClientSpy { AssociatedCompanies = [] };
        var secureEdgeApiClient = new SecureEdgeApiClientSpy();
        var authContext =  UnitTestHelpers.CreateAuthenticationContext("agent-any");
        var sut = new IxonCompanyConfigurationService(apiClient, authContext, secureEdgeApiClient);

        // Act
        var result = await sut.GetConfigurationAsync();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("No associated company found."));
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.CompanyId, Is.Null);
            Assert.That(result.Data.AgentId, Is.Null);

            Assert.That(apiClient.GetAssociatedCompanyCallCount, Is.EqualTo(1));
            Assert.That(secureEdgeApiClient.GetSystemInfoCallCount, Is.EqualTo(0));
            Assert.That(apiClient.GetAgentsCallCount, Is.EqualTo(0));
        });
    }

    [Test]
    public async Task GetConfigurationAsync_WhenSecureEdgeSystemInfoFails_ReturnsErrorAndStopsBeforeAgentLookup()
    {
        // Arrange
        var apiClient = new ApiClientSpy { AssociatedCompanies = [new Company { PublicId = "comp-1" }] };
        var secureEdgeApiClient = new SecureEdgeApiClientSpy
        {
            SystemInfoResult = new Result<SecureEdgeSystemInfo>
            {
                ErrorMessage = "Secure Edge unavailable",
            },
        };
        var authContext = UnitTestHelpers.CreateAuthenticationContext("agent-any");
        var sut = new IxonCompanyConfigurationService(apiClient, authContext, secureEdgeApiClient);

        // Act
        var result = await sut.GetConfigurationAsync();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Secure Edge unavailable"));
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.CompanyId, Is.EqualTo("comp-1"));
            Assert.That(result.Data.AgentId, Is.Null);
            Assert.That(authContext.IxonHeaders.CompanyId, Is.EqualTo("comp-1"));

            Assert.That(apiClient.GetAssociatedCompanyCallCount, Is.EqualTo(1));
            Assert.That(secureEdgeApiClient.GetSystemInfoCallCount, Is.EqualTo(1));
            Assert.That(apiClient.GetAgentsCallCount, Is.EqualTo(0));
        });
    }

    [Test]
    public async Task GetConfigurationAsync_WhenNoMatchingAgent_ReturnsError()
    {
        // Arrange
        var apiClient = new ApiClientSpy
        {
            AssociatedCompanies = [new Company { PublicId = "comp-1" }],
            Agents = [new Agent { PublicId = "agent-1", DeviceId = "OTHER-DEVICE" }],
        };
        var secureEdgeApiClient = new SecureEdgeApiClientSpy
        {
            SystemInfoResult = new Result<SecureEdgeSystemInfo>
            {
                Data = new SecureEdgeSystemInfo { SerialNumber = "SN-123456" },
            },
        };
        var authContext = UnitTestHelpers.CreateAuthenticationContext("agent-any");
        var sut = new IxonCompanyConfigurationService(apiClient, authContext, secureEdgeApiClient);

        // Act
        var result = await sut.GetConfigurationAsync();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("No agent found in IXON with a device ID containing the device serial number."));
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.CompanyId, Is.EqualTo("comp-1"));
            Assert.That(result.Data.AgentId, Is.Null);
        });
    }
}
