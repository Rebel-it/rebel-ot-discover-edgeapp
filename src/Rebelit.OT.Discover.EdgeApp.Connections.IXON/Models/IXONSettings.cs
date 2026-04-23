using System.ComponentModel.DataAnnotations;

namespace Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;

public class IXONSettings
{
    [Required]
    public required string ApplicationId { get; set; }
    [Required]
    public required string CompanyId { get; set; }
    [Required]
    public required string AgentId { get; set; }
    public string? DataSourceId { get; set; }
}
