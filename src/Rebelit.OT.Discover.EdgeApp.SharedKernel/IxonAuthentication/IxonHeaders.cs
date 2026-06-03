namespace Rebelit.OT.Discover.EdgeApp.SharedKernel.IxonAuthentication;

public class IxonHeaders
{
    /// <summary>
    ///     The service account as set up in IXON Cloud.
    /// </summary>
    public required ServiceAccount ServiceAccount { get; set; }
    
    /// <summary>
    ///     The Company ID that is linked with the IXON service account.
    /// </summary>
    public string? CompanyId { get; set; }
    
    /// <summary>
    ///     The Agent ID that represents the IXON device.
    /// </summary>
    public string? AgentId { get; set; }

    /// <summary>
    ///     The URL of the PLC.
    /// </summary>
    public string? PlcUrl { get; set; }

    /// <summary>
    ///     The username for authenticating with the PLC.
    /// </summary>
    public string? PlcUsername { get; set; }

    /// <summary>
    ///     The password for authenticating with the PLC.
    /// </summary>
    public string? PlcPassword { get; set; }

    /// <summary>
    /// Sourceid of the opcua datasource
    /// </summary>
    public string? SourceId { get; set; }

    public string GetRequiredAgentId()
    {
        if(string.IsNullOrEmpty(AgentId))
        {
            throw new InvalidOperationException("AgentId is required but was not set.");
        } 
        return AgentId;
    }

    public string GetRequiredCompanyId()
    {
        if (string.IsNullOrEmpty(CompanyId))
        {
            throw new InvalidOperationException("CompanyId is required but was not set.");
        }
        return CompanyId;
    }

    public string GetRequiredPlcUrl()
    {
        if (string.IsNullOrEmpty(PlcUrl))
        {
            throw new InvalidOperationException("PlcUrl is required but was not set.");
        }
        return PlcUrl;
    }

    public string GetRequiredSourceId()
    {
        if (string.IsNullOrEmpty(SourceId))
        {
            throw new InvalidOperationException("SourceId is required but was not set.");
        }
        return SourceId;
    }

    public string GetRequiredPlcUsername()
    {
        if (string.IsNullOrEmpty(PlcUsername))
        {
            throw new InvalidOperationException("PlcUsername is required but was not set.");
        }
        return PlcUsername;
    }

    public string GetRequiredPlcPassword()
    {
        if (string.IsNullOrEmpty(PlcPassword))
        {
            throw new InvalidOperationException("PlcPassword is required but was not set.");
        }
        return PlcPassword;
    }


}