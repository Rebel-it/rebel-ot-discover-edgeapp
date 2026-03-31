using Microsoft.Extensions.Logging;
using Opc.Ua;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;

namespace Rebelit.OT.Discover.EdgeApp.Mappers;

public interface IOpcUaVariableMapper
{
    Variable? Map(ReferenceDescription referenceDescription, string dataSourceId);
}

internal sealed class OpcUaVariableMapper(ILogger<OpcUaVariableMapper> logger)
    : IOpcUaVariableMapper
{
    private static readonly Dictionary<BuiltInType, (string Type, string? Width)> TypeMap = new()
    {
        [BuiltInType.Boolean] = ("bool", "1"),
        [BuiltInType.SByte] = ("sbyte", "8"),
        [BuiltInType.Byte] = ("byte", "8"),
        [BuiltInType.Int16] = ("short", "16"),
        [BuiltInType.UInt16] = ("ushort", "16"),
        [BuiltInType.Int32] = ("int", "32"),
        [BuiltInType.UInt32] = ("uint", "32"),
        [BuiltInType.Int64] = ("long", "64"),
        [BuiltInType.UInt64] = ("ulong", "64"),
        [BuiltInType.Float] = ("float", "32"),
        [BuiltInType.Double] = ("double", "64"),
        [BuiltInType.String] = ("string", null),
    };

    public Variable? Map(ReferenceDescription referenceDescription, string dataSourceId)
    {
        var builtInType = ResolveBuiltInType(referenceDescription.TypeDefinition);
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

        var (type, width) = mapping;

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
            Width = width ?? "Unknown",
            Slug = new string([
                .. referenceDescription.DisplayName.ToString().Where(char.IsLetterOrDigit),
            ]).ToLower(),
            Source = new Source { PublicId = dataSourceId },
            Signed = true,
        };
    }

    private static BuiltInType? ResolveBuiltInType(ExpandedNodeId dataTypeId)
    {
        if (dataTypeId.IdType != IdType.Numeric)
            return null;

        uint numericId = (uint)dataTypeId.Identifier;
        if (numericId >= 1 && numericId <= 25)
            return (BuiltInType)numericId;

        return null;
    }
}
