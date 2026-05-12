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

        var companyResult = await apiClient.GetAssociatedCompanyAsync();

        if (companyResult.HasError)
        {
            response.ErrorMessage = companyResult.ErrorMessage ?? "The company could not be retrieved from IXON.";
            return response;
        }

        var company = companyResult.Data?.FirstOrDefault();

        if (company == null)
        {
            response.ErrorMessage = "No associated company found.";
            return response;
        }

        authenticationContext.IxonHeaders.CompanyId = company.PublicId;
        response.Data.CompanyId = company.PublicId;

        var deviceSystemInfoResult = await secureEdgeApiClient.GetSystemInfoAsync();
        var deviceSystemInfo = deviceSystemInfoResult.Data;

        if (!deviceSystemInfoResult.Success)
        {
            response.ErrorMessage = deviceSystemInfoResult.ErrorMessage;
            return response;
        }

        if (string.IsNullOrEmpty(deviceSystemInfo?.SerialNumber))
        {
            response.ErrorMessage = "Serial number of device is missing from system info response.";
            return response;
        }

        var agentResult = await apiClient.GetAgentsAsync();

        if (agentResult.HasError)
        {
            response.ErrorMessage = agentResult.ErrorMessage ?? "The agent could not be retrieved from IXON.";
            return response;
        }
        var agents = agentResult.Data?.ToArray();

        if (agents is not { Length: > 0 })
        {
            response.ErrorMessage = "It seems there are no agents in the company.";
            return response;
        }

        var agent = agents.FirstOrDefault(x => x.DeviceId != null && x.DeviceId.Contains(deviceSystemInfo.SerialNumber));
        if (agent == null)
        {
            response.ErrorMessage = "No agent found in IXON with a device ID containing the device serial number.";
            return response;
        }

        response.Data.AgentId = agent.PublicId;
        return response;
    }
}