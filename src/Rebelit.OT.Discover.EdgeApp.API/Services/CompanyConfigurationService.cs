using Rebelit.OT.Discover.EdgeApp.API.Dto;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Agents;
using Rebelit.OT.Discover.EdgeApp.SharedKernel;
using Rebelit.OT.Discover.EdgeApp.SharedKernel.IxonAuthentication;

namespace Rebelit.OT.Discover.EdgeApp.API.Services;

public class CompanyConfigurationService(IApiClient apiClient, IIxonAuthenticationContext authenticationContext) : ICompanyConfigurationService
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

        var agent = agents[0];
        response.Data.AgentId = agent.PublicId;
        
        return response;
    }
}