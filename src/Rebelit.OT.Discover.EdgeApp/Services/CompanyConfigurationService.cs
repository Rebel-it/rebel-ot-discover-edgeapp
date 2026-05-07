using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Agents;
using Rebelit.OT.Discover.EdgeApp.Dto;
using Rebelit.OT.Discover.EdgeApp.SharedKernel.IxonAuthentication;

namespace Rebelit.OT.Discover.EdgeApp.Services;

public class CompanyConfigurationService(IApiClient apiClient, IIxonAuthenticationContext authenticationContext) : ICompanyConfigurationService
{
    public async Task<ResponseDto<CompanyConfigurationDto>> GetConfigurationAsync()
    {
        var response = new ResponseDto<CompanyConfigurationDto>
        {
            Data = new CompanyConfigurationDto()
        };

        var companyResult = await apiClient.GetAssociatedCompanyAsync();
        
        if (companyResult.Error != null)
        {
            response.ErrorMessage = companyResult.ErrorMessage;
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
        
        if (agentResult.Error != null)
        {
            response.ErrorMessage = agentResult.ErrorMessage;
            return response;
        }
        var agents = agentResult.Data?.ToArray();
        
        if (agents is { Length: 0 })
        {
            response.ErrorMessage = "It seems there are no agents in the company.";
            return response;
        }

        var agent = agents![0];
        response.Data.AgentId = agent.PublicId;
        
        return response;
    }
}