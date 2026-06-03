using Opc.Ua;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;

namespace Rebelit.OT.Discover.EdgeApp.API.Mappers;

public interface IOpcUaVariableMapper
{
    Variable? Map(NodeId dataTypeNodeId, ReferenceDescription referenceDescription, string dataSourceId);
}