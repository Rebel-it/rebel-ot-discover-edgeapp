using Rebelit.OT.Discover.EdgeApp.Dto;

namespace Rebelit.OT.Discover.EdgeApp.Services;

public interface ICompanyConfigurationService
{
    Task<CompanyConfigurationDto?> GetConfigurationAsync();
}