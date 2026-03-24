using System.Text.Json.Serialization;

namespace Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;

/// <summary>
///     Represents the protocol of a data source in the IXON API, identified by a public ID.
/// </summary>
/// <remarks>
///     Known protocol public IDs include: <c>custom</c>, <c>modbus</c>, <c>siemens-step7</c>,
///     <c>opc-ua</c>, <c>ethernet-ip</c>, <c>host</c>, <c>bacnet-ip</c>, <c>mc-protocol</c>.
/// </remarks>
public class DataSourceProtocol
{
    [JsonPropertyName("publicId")]
    public string PublicId { get; set; } = null!;
}
