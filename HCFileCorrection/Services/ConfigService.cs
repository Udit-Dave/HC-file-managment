using HCFileCorrection.Entities;
using HCFileCorrection.Interfaces;
using HCFileCorrection.Models;
using HCFileCorrection.Utility;

namespace HCFileCorrection.Services
{
    public class ConfigService : IConfigService
    {
        private readonly IConfigRepository _configRepository;
        private readonly IConfiguration _configuration;
        public ConfigService(IConfigRepository configRepository, IConfiguration configuration)
        {
            _configuration = configuration;
            _configRepository = configRepository;
        }

        public async Task<List<Country>> GetCountriesAsync(List<int>? countryId)
        {
            return await _configRepository.GetCountryAsync(countryId);
        }

        public async Task<List<Role>> GetRolesAsync()
        {
            return await _configRepository.GetRolesAsync();
        }
        public async Task<List<RetailerModel>> GetRetailers()
        {
            return await _configRepository.GetRetailersAsync();
        }
    }
}
