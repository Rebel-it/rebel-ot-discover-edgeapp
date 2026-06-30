using Microsoft.AspNetCore.Mvc;
using Rebelit.OT.Discover.EdgeApp.API.Controllers;
using Rebelit.OT.Discover.EdgeApp.API.Dto;
using Rebelit.OT.Discover.EdgeApp.API.Services;
using Rebelit.OT.Discover.EdgeApp.SharedKernel;

namespace Rebelit.OT.Discover.EdgeApp.Tests.Controllers;

[TestFixture]
public class CompanyConfigurationControllerTests
{
    [Test]
    public async Task GetConfiguration_WhenServiceSucceeds_ReturnsOkWithData()
    {
        // Arrange
        var dto = new CompanyConfigurationDto { CompanyId = "comp-1", AgentId = "agent-1" };
        var service = new FakeIxonCompanyConfigurationService(new Result<CompanyConfigurationDto> { Data = dto });
        var sut = new CompanyConfigurationController(service);

        // Act
        var result = await sut.GetConfiguration();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(service.CallCount, Is.EqualTo(1));

            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult!.Value, Is.EqualTo(dto));
        });
    }

    [Test]
    public async Task GetConfiguration_WhenServiceFails_ReturnsBadRequestWithErrorMessage()
    {
        // Arrange
        const string error = "No associated company found.";
        var service = new FakeIxonCompanyConfigurationService(new Result<CompanyConfigurationDto> { ErrorMessage = error });
        var sut = new CompanyConfigurationController(service);

        // Act
        var result = await sut.GetConfiguration();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(service.CallCount, Is.EqualTo(1));

            var badRequestResult = result as BadRequestObjectResult;
            Assert.That(badRequestResult, Is.Not.Null);
            Assert.That(badRequestResult!.Value, Is.EqualTo(error));
        });
    }

    private sealed class FakeIxonCompanyConfigurationService(Result<CompanyConfigurationDto> result)
        : IIxonCompanyConfigurationService
    {
        public int CallCount { get; private set; }

        public Task<Result<CompanyConfigurationDto>> GetConfigurationAsync()
        {
            CallCount++;
            return Task.FromResult(result);
        }
    }
}
