using Rebelit.OT.Discover.EdgeApp.API.Dto;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Agents;
using Rebelit.OT.Discover.EdgeApp.SharedKernel;
using Rebelit.OT.Discover.EdgeApp.SharedKernel.IxonAuthentication;

namespace Rebelit.OT.Discover.EdgeApp.API.Services;

public class CompanyConfigurationService(
    IApiClient apiClient, 
    IIxonAuthenticationContext authenticationContext,
    Connections.SecureEdgePro.IApiClient secureEdgeApiClient) : ICompanyConfigurationService
{
    public async Task<Result<CompanyConfigurationDto>> GetConfigurationAsync()
    {
        var response = new Result<CompanyConfigurationDto>
        {
            Data = new CompanyConfigurationDto()
        };

        var companyId = await GetCompanyIdAsync(response);
        if (companyId is null)
        {
            return response;
        }

        authenticationContext.IxonHeaders.CompanyId = companyId;
        response.Data.CompanyId = companyId;

        var serialNumber = await GetDeviceSerialNumberAsync(response);
        if (serialNumber is null)
        {
            return response;
        }

        var agentId = await GetAgentIdAsync(serialNumber, response);
        if (agentId is null)
        {
            return response;
        }

        response.Data.AgentId = agentId;
        return response;
    }

    private async Task<string?> GetCompanyIdAsync(Result<CompanyConfigurationDto> response)
    {
        var companyResult = await apiClient.GetAssociatedCompanyAsync();

        if (companyResult.HasError)
        {
            response.ErrorMessage = companyResult.ErrorMessage ?? "The company could not be retrieved from IXON.";
            return null;
        }

        var companyId = companyResult.Data?.FirstOrDefault()?.PublicId;
        if (string.IsNullOrEmpty(companyId))
        {
            response.ErrorMessage = "No associated company found.";
            return null;
        }

        return companyId;
    }

    private async Task<string?> GetDeviceSerialNumberAsync(Result<CompanyConfigurationDto> response)
    {
        var deviceSystemInfoResult = await secureEdgeApiClient.GetSystemInfoAsync();

        if (!deviceSystemInfoResult.Success)
        {
            response.ErrorMessage = deviceSystemInfoResult.ErrorMessage;
            return null;
        }

        var serialNumber = deviceSystemInfoResult.Data?.SerialNumber;
        if (string.IsNullOrEmpty(serialNumber))
        {
            response.ErrorMessage = "Serial number of device is missing from system info response.";
            return null;
        }

        return serialNumber;
    }

    private async Task<string?> GetAgentIdAsync(string serialNumber, Result<CompanyConfigurationDto> response)
    {
        var agentResult = await apiClient.GetAgentsAsync();

        if (agentResult.HasError)
        {
            response.ErrorMessage = agentResult.ErrorMessage ?? "The agent could not be retrieved from IXON.";
            return null;
        }

        var agent = agentResult.Data?
            .FirstOrDefault(x => x.DeviceId is not null && x.DeviceId.Contains(serialNumber, StringComparison.InvariantCultureIgnoreCase));

        if (agent is null)
        {
            response.ErrorMessage = "No agent found in IXON with a device ID containing the device serial number.";
            return null;
        }

        return agent.PublicId;
    }
}