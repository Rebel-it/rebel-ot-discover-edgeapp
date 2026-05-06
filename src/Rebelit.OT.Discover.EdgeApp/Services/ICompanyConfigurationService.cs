using Rebelit.OT.Discover.EdgeApp.Dto;

namespace Rebelit.OT.Discover.EdgeApp.Services;

public interface ICompanyConfigurationService
{
    Task<CompanyConfiguration?> GetConfigurationAsync(ServiceAccountDto serviceAccount);
}