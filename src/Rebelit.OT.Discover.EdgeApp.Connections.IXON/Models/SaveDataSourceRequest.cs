using System.ComponentModel.DataAnnotations;

namespace Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;

public class SaveDataSourceRequest
{
    [Required]
    public required string DataSourceName { get; set; }

    [Required]
    public required string AgentId { get; set; }
}
