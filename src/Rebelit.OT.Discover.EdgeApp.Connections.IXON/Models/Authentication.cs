using System;
using System.Collections.Generic;
using System.Text;

namespace Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;

public class Authentication
{
    public required string Username { get; set; }
    public required string Password { get; set; }
    public string? OtpCode { get; set; }
    public required string ApplicationID { get; set; }
}
