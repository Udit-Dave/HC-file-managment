using HCFileCorrection.Models;

namespace HCFileCorrection.Interfaces
{
    public interface IConfigService
    {
        Task<List<Country>> GetCountriesAsync(List<int>? countryId);
        Task<List<Role>> GetRolesAsync();

        Task<List<RetailerModel>> GetRetailers();
    }
}
