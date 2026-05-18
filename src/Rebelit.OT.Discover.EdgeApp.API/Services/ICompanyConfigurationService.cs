using Rebelit.OT.Discover.EdgeApp.API.Dto;
using Rebelit.OT.Discover.EdgeApp.SharedKernel;

namespace Rebelit.OT.Discover.EdgeApp.API.Services;

public interface ICompanyConfigurationService
{
    Task<Result<CompanyConfigurationDto>> GetConfigurationAsync();
}