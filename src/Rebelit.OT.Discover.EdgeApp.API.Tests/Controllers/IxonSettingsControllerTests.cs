using Microsoft.AspNetCore.Mvc;
using Rebelit.OT.Discover.EdgeApp.API.Controllers;
using Rebelit.OT.Discover.EdgeApp.API.Resolvers;
using Rebelit.OT.Discover.EdgeApp.API.Services;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;
using Rebelit.OT.Discover.EdgeApp.SharedKernel;

namespace Rebelit.OT.Discover.EdgeApp.Tests.Controllers;

[TestFixture]
public class IxonSettingsControllerTests
{
    [Test]
    public async Task PostNewSource_WhenResolverSucceeds_ReturnsOkWithData()
    {
        // Arrange
        var resolver = new FakeDataSourceResolver(new Result<string> { Data = "resolved-source" });
        var settingService = new FakeIxonSettingService("success");
        var sut = new IxonSettingsController(resolver, settingService);

        var request = new SaveDataSourceRequest { DataSourceName = "MachineA" };

        // Act
        var result = await sut.PostNewSource(request);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(resolver.CallCount, Is.EqualTo(1));
            Assert.That(resolver.LastSourceName, Is.EqualTo("MachineA"));

            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult!.Value, Is.EqualTo("resolved-source"));
        });
    }

    [Test]
    public async Task PostNewSource_WhenResolverFails_ReturnsBadRequestWithErrorMessage()
    {
        // Arrange
        const string error = "Could not resolve source.";
        var resolver = new FakeDataSourceResolver(new Result<string> { ErrorMessage = error });
        var settingService = new FakeIxonSettingService("success");
        var sut = new IxonSettingsController(resolver, settingService);

        var request = new SaveDataSourceRequest { DataSourceName = "MachineA" };

        // Act
        var result = await sut.PostNewSource(request);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(resolver.CallCount, Is.EqualTo(1));
            Assert.That(resolver.LastSourceName, Is.EqualTo("MachineA"));

            var badRequestResult = result as BadRequestObjectResult;
            Assert.That(badRequestResult, Is.Not.Null);
            Assert.That(badRequestResult!.Value, Is.EqualTo(error));
        });
    }

    [Test]
    public async Task PushDeviceConfiguration_WhenPushSucceeds_ReturnsOkWithResult()
    {
        // Arrange
        var resolver = new FakeDataSourceResolver(new Result<string> { Data = "unused" });
        var settingService = new FakeIxonSettingService("success");
        var sut = new IxonSettingsController(resolver, settingService);

        // Act
        var result = await sut.PushDeviceConfiguration();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(settingService.CallCount, Is.EqualTo(1));

            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult!.Value, Is.EqualTo("success"));
        });
    }

    [Test]
    public async Task PushDeviceConfiguration_WhenPushFails_ReturnsBadRequestWithErrorMessage()
    {
        // Arrange
        var resolver = new FakeDataSourceResolver(new Result<string> { Data = "unused" });
        var settingService = new FakeIxonSettingService("failed");
        var sut = new IxonSettingsController(resolver, settingService);

        // Act
        var result = await sut.PushDeviceConfiguration();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(settingService.CallCount, Is.EqualTo(1));

            var badRequestResult = result as BadRequestObjectResult;
            Assert.That(badRequestResult, Is.Not.Null);
            Assert.That(badRequestResult!.Value, Is.EqualTo("Failed to push device configuration."));
        });
    }

    private sealed class FakeDataSourceResolver(Result<string> result) : IDataSourceResolver
    {
        public int CallCount { get; private set; }
        public string? LastSourceName { get; private set; }

        public Task<Result<string>> ResolveAsync(string sourceName)
        {
            CallCount++;
            LastSourceName = sourceName;
            return Task.FromResult(result);
        }
    }

    private sealed class FakeIxonSettingService(string? result) : IIxonSettingService
    {
        public int CallCount { get; private set; }

        public Task<string?> PushDeviceConfigAsync()
        {
            CallCount++;
            return Task.FromResult(result);
        }
    }
}
