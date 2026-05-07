using Rebelit.OT.Discover.EdgeApp.Dto;
using Rebelit.OT.Discover.EdgeApp.SharedKernel;

namespace Rebelit.OT.Discover.EdgeApp.Services;

public interface ICompanyConfigurationService
{
    Task<Result<CompanyConfigurationDto>> GetConfigurationAsync();
}