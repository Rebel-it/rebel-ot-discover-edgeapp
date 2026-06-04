using Microsoft.Extensions.Logging;
using Opc.Ua;
using Rebelit.OT.Discover.EdgeApp.API.Mappers;
using Rebelit.OT.Discover.EdgeApp.Tests.Services.Helpers;

namespace Rebelit.OT.Discover.EdgeApp.Tests.Mapper;

[TestFixture]
public class OpcUaVariableMapperTests
{
    [Test]
    public void Map_WhenDataTypeIsInt32_ReturnsMappedVariable()
    {
        // Arrange
        var logger = new TestLogger<OpcUaVariableMapper>(isInfoEnabled: true);
        var sut = new OpcUaVariableMapper(logger);
        var nodeId = new NodeId((uint)BuiltInType.Int32);
        var referenceDescription = CreateReferenceDescription("Temperature", "ns=2;s=Temp");

        // Act
        var result = sut.Map(nodeId, referenceDescription, "source-1");

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Name, Is.EqualTo("Temperature"));
            Assert.That(result.Address, Is.EqualTo("ns=2;s=Temp"));
            Assert.That(result.Type, Is.EqualTo("int"));
            Assert.That(result.Width, Is.EqualTo("32"));
            Assert.That(result.MaxStringLength, Is.Null);
            Assert.That(result.Source.PublicId, Is.EqualTo("source-1"));
            Assert.That(result.Slug, Is.EqualTo("temperature_stemp"));
            Assert.That(result.Signed, Is.True);
            Assert.That(logger.Entries.Any(e => e.LogLevel == LogLevel.Warning), Is.False);
        });
    }

    [Test]
    public void Map_WhenDataTypeIsString_ReturnsMappedVariableWithMaxStringLengthAndLogsWidthWarning()
    {
        // Arrange
        var logger = new TestLogger<OpcUaVariableMapper>(isInfoEnabled: true);
        var sut = new OpcUaVariableMapper(logger);
        var nodeId = new NodeId((uint)BuiltInType.String);
        var referenceDescription = CreateReferenceDescription("Status Text", "ns=2;s=StatusText");

        // Act
        var result = sut.Map(nodeId, referenceDescription, "source-1");

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Type, Is.EqualTo("str"));
            Assert.That(result.Width, Is.Null);
            Assert.That(result.MaxStringLength, Is.EqualTo(128));
            Assert.That(logger.Entries.Any(e => e.LogLevel == LogLevel.Warning && e.Message.Contains("does not have a defined width")), Is.True);
        });
    }

    [Test]
    public void Map_WhenDataTypeIdTypeIsNotNumeric_ReturnsNullAndLogsWarning()
    {
        // Arrange
        var logger = new TestLogger<OpcUaVariableMapper>(isInfoEnabled: true);
        var sut = new OpcUaVariableMapper(logger);
        var nodeId = new NodeId("not-numeric", 2);
        var referenceDescription = CreateReferenceDescription("Temperature", "ns=2;s=Temp");

        // Act
        var result = sut.Map(nodeId, referenceDescription, "source-1");

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Null);
            Assert.That(logger.Entries.Any(e => e.LogLevel == LogLevel.Warning && e.Message.Contains("could not be mapped: TypeDefinition NodeId is not a known numeric type")), Is.True);
        });
    }

    [Test]
    public void Map_WhenNumericTypeIsOutOfBuiltInRange_ReturnsNullAndLogsWarning()
    {
        // Arrange
        var logger = new TestLogger<OpcUaVariableMapper>(isInfoEnabled: true);
        var sut = new OpcUaVariableMapper(logger);
        var nodeId = new NodeId(99u);
        var referenceDescription = CreateReferenceDescription("Temperature", "ns=2;s=Temp");

        // Act
        var result = sut.Map(nodeId, referenceDescription, "source-1");

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Null);
            Assert.That(logger.Entries.Any(e => e.LogLevel == LogLevel.Warning && e.Message.Contains("could not be mapped: TypeDefinition NodeId is not a known numeric type")), Is.True);
        });
    }

    private static ReferenceDescription CreateReferenceDescription(string displayName, string nodeId) =>
        new()
        {
            DisplayName = new LocalizedText(displayName),
            NodeId = new ExpandedNodeId(nodeId),
        };
}
