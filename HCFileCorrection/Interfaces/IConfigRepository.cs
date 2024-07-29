using HCFileCorrection.Models;

namespace HCFileCorrection.Interfaces
{
    public interface IConfigRepository
    {
        Task<List<Country>> GetCountryAsync(List<int>? countryId);
        Task<List<Role>> GetRolesAsync();

        Task<List<RetailerModel>> GetRetailersAsync();
    }
}
