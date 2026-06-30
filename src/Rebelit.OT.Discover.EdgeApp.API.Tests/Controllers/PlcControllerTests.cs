using System.Runtime.Serialization;
using Microsoft.AspNetCore.Mvc;
using Rebelit.OT.Discover.EdgeApp.API.Controllers;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;
using Rebelit.OT.Discover.EdgeApp.Connections.OPCUA.Clients;
using Rebelit.OT.Discover.EdgeApp.Connections.OPCUA.Factory;

namespace Rebelit.OT.Discover.EdgeApp.Tests.Controllers;

[TestFixture]
public class PlcControllerTests
{
    [Test]
    public async Task TestOpcConnection_WhenFactoryReturnsClient_ReturnsOk()
    {
        // Arrange
        var factory = new FakeUaClientFactory(CreateDisposableUaClient());
        var sut = new PlcController(factory);

        var settings = new OpcCredentials
        {
            OpcUaServerAddress = "192.168.1.1:4840",
            OpcUaUsername = "user1",
            OpcUaPassword = "pass1",
        };

        // Act
        var result = await sut.TestOpcConnection(settings);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(factory.CallCount, Is.EqualTo(1));
            Assert.That(factory.LastServerAddress, Is.EqualTo("192.168.1.1:4840"));
            Assert.That(factory.LastUsername, Is.EqualTo("user1"));
            Assert.That(factory.LastPassword, Is.EqualTo("pass1"));

            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult!.Value, Is.EqualTo("Successfully connected with the OPC UA server."));
        });
    }

    [Test]
    public async Task TestOpcConnection_WhenFactoryReturnsNull_ReturnsBadRequest()
    {
        // Arrange
        var factory = new FakeUaClientFactory(null);
        var sut = new PlcController(factory);

        var settings = new OpcCredentials
        {
            OpcUaServerAddress = "192.168.1.1:4840",
            OpcUaUsername = null,
            OpcUaPassword = null,
        };

        // Act
        var result = await sut.TestOpcConnection(settings);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(factory.CallCount, Is.EqualTo(1));
            Assert.That(factory.LastServerAddress, Is.EqualTo("192.168.1.1:4840"));
            Assert.That(factory.LastUsername, Is.EqualTo(string.Empty));
            Assert.That(factory.LastPassword, Is.EqualTo(string.Empty));

            var badRequestResult = result as BadRequestObjectResult;
            Assert.That(badRequestResult, Is.Not.Null);
            Assert.That(
                badRequestResult!.Value,
                Is.EqualTo("Failed to connect to the OPC UA server. Please validate the service address and credentials if applicable.")
            );
        });
    }

    private static UAClient CreateDisposableUaClient()
    {
        var client = (UAClient)FormatterServices.GetUninitializedObject(typeof(UAClient));
        typeof(UAClient).GetField("_disposed", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .SetValue(client, true);
        return client;
    }

    private sealed class FakeUaClientFactory(UAClient? clientToReturn) : IUAClientFactory
    {
        public int CallCount { get; private set; }
        public string? LastServerAddress { get; private set; }
        public string? LastUsername { get; private set; }
        public string? LastPassword { get; private set; }

        public Task<UAClient?> CreateAsync(string uri)
        {
            CallCount++;
            LastServerAddress = uri;
            return Task.FromResult(clientToReturn);
        }

        public Task<UAClient?> CreateAsync(string opcServerAddress, string username, string password)
        {
            CallCount++;
            LastServerAddress = opcServerAddress;
            LastUsername = username;
            LastPassword = password;
            return Task.FromResult(clientToReturn);
        }
    }
}
