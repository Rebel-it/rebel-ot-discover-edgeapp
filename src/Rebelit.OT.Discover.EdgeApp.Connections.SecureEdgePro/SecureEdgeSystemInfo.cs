using System.Text.Json.Serialization;

namespace Rebelit.OT.Discover.EdgeApp.Connections.SecureEdgePro;

public class SecureEdgeSystemInfo
{
    [JsonPropertyName("serial_number")]
    public string? SerialNumber { get; set; }
    
    [JsonPropertyName("lan_mac_address")]
    public string? LanMacAddress { get; set; }
    
    [JsonPropertyName("wan_mac_address")]
    public string? WanMacAddress { get; set; }
    
    [JsonPropertyName("wan2_mac_address")]
    public string? Wan2MacAddress { get; set; }
    
    [JsonPropertyName("wlan_mac_address")]
    public string? WlanMacAddress { get; set; }
}