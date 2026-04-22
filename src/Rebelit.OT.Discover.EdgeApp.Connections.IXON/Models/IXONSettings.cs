using System;
using System.Collections.Generic;
using System.Text;

namespace Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;

public class IXONSettings
{
    public required string ApplicationId { get; set; }
    public required string ComnpanyId { get; set; }
    public required string AgentId { get; set; }
    public string? DataSourceID { get; set; }
    public required string OpcUaServerAddress { get; set; }
    public required string OpcUaUsername { get; set; }
    public required string OpcUaPassword { get; set; }
}
