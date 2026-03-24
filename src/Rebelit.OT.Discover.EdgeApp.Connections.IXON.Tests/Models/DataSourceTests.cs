using System.Text.Json;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;

namespace Rebelit.OT.Discover.EdgeApp.Connections.IXON.Tests.Models;

[TestFixture]
public class DataSourceTests
{
    [Test]
    public void AllNullableProperties_ByDefault_AreNull()
    {
        var dataSource = new DataSource
        {
            Name = "n",
            Slug = "s",
            Device = new Source { PublicId = "d-1" },
            Protocol = new DataSourceProtocol { PublicId = "modbus" },
        };

        Assert.Multiple(() =>
        {
            Assert.That(dataSource.PublicId, Is.Null);
            Assert.That(dataSource.Disabled, Is.Null);
            Assert.That(dataSource.ContinueLoggingOnError, Is.Null);
            Assert.That(dataSource.GracePeriod, Is.Null);
            Assert.That(dataSource.Port, Is.Null);
            Assert.That(dataSource.PullDelay, Is.Null);
            Assert.That(dataSource.MqttTopic, Is.Null);
            Assert.That(dataSource.Agent, Is.Null);
            Assert.That(dataSource.MqttBrokerClient, Is.Null);
        });
    }

    [Test]
    public void AllProperties_WhenAssigned_ReturnAssignedValues()
    {
        var device = new Source { PublicId = "dev-1" };
        var agent = new Source { PublicId = "agt-1" };
        var broker = new Source { PublicId = "brk-1" };
        var protocol = new DataSourceProtocol { PublicId = "modbus" };

        var dataSource = new DataSource
        {
            PublicId = "ds-1",
            Name = "My Source",
            Slug = "my-source",
            Disabled = false,
            ContinueLoggingOnError = true,
            GracePeriod = 30,
            Port = 502,
            PullDelay = 1000L,
            MqttTopic = "sensors/temp",
            Device = device,
            Agent = agent,
            MqttBrokerClient = broker,
            Protocol = protocol,
        };

        Assert.Multiple(() =>
        {
            Assert.That(dataSource.PublicId, Is.EqualTo("ds-1"));
            Assert.That(dataSource.Name, Is.EqualTo("My Source"));
            Assert.That(dataSource.Slug, Is.EqualTo("my-source"));
            Assert.That(dataSource.Disabled, Is.False);
            Assert.That(dataSource.ContinueLoggingOnError, Is.True);
            Assert.That(dataSource.GracePeriod, Is.EqualTo(30));
            Assert.That(dataSource.Port, Is.EqualTo(502));
            Assert.That(dataSource.PullDelay, Is.EqualTo(1000L));
            Assert.That(dataSource.MqttTopic, Is.EqualTo("sensors/temp"));
            Assert.That(dataSource.Device, Is.SameAs(device));
            Assert.That(dataSource.Agent, Is.SameAs(agent));
            Assert.That(dataSource.MqttBrokerClient, Is.SameAs(broker));
            Assert.That(dataSource.Protocol, Is.SameAs(protocol));
        });
    }

    [Test]
    public void Deserialize_WithAllFields_MapsAllProperties()
    {
        const string json = """
            {
              "publicId": "ds-1",
              "name": "My Source",
              "slug": "my-source",
              "disabled": false,
              "continueLoggingOnError": true,
              "gracePeriod": 30,
              "port": 502,
              "pullDelay": 1000,
              "mqttTopic": "sensors/temp",
              "device": { "publicId": "dev-1" },
              "agent": { "publicId": "agt-1" },
              "mqttBrokerClient": { "publicId": "brk-1" },
              "protocol": { "publicId": "modbus" }
            }
            """;

        var result = JsonSerializer.Deserialize<DataSource>(json)!;

        Assert.Multiple(() =>
        {
            Assert.That(result.PublicId, Is.EqualTo("ds-1"));
            Assert.That(result.Name, Is.EqualTo("My Source"));
            Assert.That(result.Slug, Is.EqualTo("my-source"));
            Assert.That(result.Disabled, Is.False);
            Assert.That(result.ContinueLoggingOnError, Is.True);
            Assert.That(result.GracePeriod, Is.EqualTo(30));
            Assert.That(result.Port, Is.EqualTo(502));
            Assert.That(result.PullDelay, Is.EqualTo(1000L));
            Assert.That(result.MqttTopic, Is.EqualTo("sensors/temp"));
            Assert.That(result.Device.PublicId, Is.EqualTo("dev-1"));
            Assert.That(result.Agent!.PublicId, Is.EqualTo("agt-1"));
            Assert.That(result.MqttBrokerClient!.PublicId, Is.EqualTo("brk-1"));
            Assert.That(result.Protocol.PublicId, Is.EqualTo("modbus"));
        });
    }

    [Test]
    public void Deserialize_WithOnlyRequiredFields_LeavesOptionalPropertiesNull()
    {
        const string json = """
            {
              "name": "Minimal",
              "slug": "minimal",
              "device": { "publicId": "dev-1" },
              "protocol": { "publicId": "opc-ua" }
            }
            """;

        var result = JsonSerializer.Deserialize<DataSource>(json)!;

        Assert.Multiple(() =>
        {
            Assert.That(result.PublicId, Is.Null);
            Assert.That(result.Disabled, Is.Null);
            Assert.That(result.GracePeriod, Is.Null);
            Assert.That(result.Port, Is.Null);
            Assert.That(result.PullDelay, Is.Null);
            Assert.That(result.MqttTopic, Is.Null);
            Assert.That(result.Agent, Is.Null);
            Assert.That(result.MqttBrokerClient, Is.Null);
        });
    }

    [Test]
    public void Serialize_WithAllFields_ProducesCorrectJson()
    {
        var dataSource = new DataSource
        {
            PublicId = "ds-1",
            Name = "My Source",
            Slug = "my-source",
            Device = new Source { PublicId = "dev-1" },
            Protocol = new DataSourceProtocol { PublicId = "modbus" },
        };

        var json = JsonSerializer.Serialize(dataSource);

        Assert.Multiple(() =>
        {
            Assert.That(json, Does.Contain("\"publicId\":\"ds-1\""));
            Assert.That(json, Does.Contain("\"name\":\"My Source\""));
            Assert.That(json, Does.Contain("\"slug\":\"my-source\""));
            Assert.That(json, Does.Contain("\"device\":{\"publicId\":\"dev-1\"}"));
            Assert.That(json, Does.Contain("\"protocol\":{\"publicId\":\"modbus\"}"));
        });
    }
}
