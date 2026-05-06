using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Agents;
using Rebelit.OT.Discover.EdgeApp.Dto;
using Rebelit.OT.Discover.EdgeApp.SharedKernel.IxonAuthentication;

namespace Rebelit.OT.Discover.EdgeApp.Services;

public class CompanyConfigurationService(IApiClient apiClient, IIxonAuthenticationContext authenticationContext) : ICompanyConfigurationService
{
    public async Task<CompanyConfiguration?> GetConfigurationAsync(ServiceAccountDto serviceAccount)
    {
        try
        {
            var company = (await apiClient.GetAssociatedCompanyAsync())?.Data;
            if (company == null)
            {
                return null;
            }

            authenticationContext.IxonCredentials.CompanyId = company.PublicId;

            var agents = (await apiClient.GetAgentsAsync()).Data;
            if(agents.Length == 0)
            {
                return null;
            }

            var agent = agents[0];

            return new CompanyConfiguration
            {
                AgentId = agent.PublicId,
                CompanyId = company.PublicId
            };
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }
}