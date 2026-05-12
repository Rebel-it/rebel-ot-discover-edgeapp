using Opc.Ua;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;

namespace Rebelit.OT.Discover.EdgeApp.API.Mappers;

public interface IOpcUaVariableMapper
{
    Variable? Map(NodeId dataTypeNodeId, ReferenceDescription referenceDescription, string dataSourceId);
}

internal sealed class OpcUaVariableMapper(ILogger<OpcUaVariableMapper> logger)
    : IOpcUaVariableMapper
{
    private static readonly Dictionary<BuiltInType, (string Type, string? Width, int? MaxStringLength)> TypeMap = new()
    {
        [BuiltInType.Boolean] = ("bool", null, null),
        [BuiltInType.SByte] = ("int", "8",null),
        [BuiltInType.Byte] = ("int", "8", null),
        [BuiltInType.Int16] = ("int", "16", null),
        [BuiltInType.UInt16] = ("int", "16", null),
        [BuiltInType.Int32] = ("int", "32", null),
        [BuiltInType.UInt32] = ("int", "32", null),
        [BuiltInType.Int64] = ("int", "64", null),
        [BuiltInType.UInt64] = ("int", "64", null),
        [BuiltInType.Float] = ("float", "32", null),
        [BuiltInType.Double] = ("float", "64", null),
        [BuiltInType.String] = ("str", null, 128),
    };

    public Variable? Map(NodeId dataTypeNodeId,ReferenceDescription referenceDescription, string dataSourceId)
    {
        var builtInType = ResolveBuiltInType(dataTypeNodeId);
        if (builtInType is null)
        {
            logger.LogWarning(
                "Node '{DisplayName}' ({NodeId}) could not be mapped: TypeDefinition NodeId is not a known numeric type.",
                referenceDescription.DisplayName,
                referenceDescription.NodeId
            );
            return null;
        }

        if (!TypeMap.TryGetValue(builtInType.Value, out var mapping))
        {
            logger.LogWarning(
                "Node '{DisplayName}' ({NodeId}) could not be mapped: built-in type '{BuiltInType}' is not supported.",
                referenceDescription.DisplayName,
                referenceDescription.NodeId,
                builtInType.Value
            );
            return null;
        }

        var (type, width, MaxStringLength) = mapping;

        if (width is null)
            logger.LogWarning(
                "The built-in type '{BuiltInType}' does not have a defined width.",
                builtInType
            );

        return new Variable
        {
            Name = referenceDescription.DisplayName.ToString(),
            Address = referenceDescription.NodeId.ToString(),
            Type = type,
            Width = width ?? null,
            MaxStringLength = MaxStringLength ?? null,
            Slug = new string([
                .. referenceDescription.DisplayName.ToString().Where(char.IsLetterOrDigit),
            ]).ToLower(),
            Source = new Source { PublicId = dataSourceId },
            Signed = true,
        };
    }

    private static BuiltInType? ResolveBuiltInType(ExpandedNodeId dataTypeId)
    {
        if (dataTypeId.IdType != IdType.Numeric || dataTypeId.Identifier == null)
            return null;

        uint numericId = (uint)dataTypeId.Identifier;
        if (numericId >= 1 && numericId <= 25)
            return (BuiltInType)numericId;

        return null;
    }
}
