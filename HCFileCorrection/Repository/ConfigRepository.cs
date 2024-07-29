using HCFileCorrection.Entities;
using HCFileCorrection.Interfaces;
using HCFileCorrection.Models;
using Microsoft.EntityFrameworkCore;

namespace HCFileCorrection.Repository
{
    public class ConfigRepository : IConfigRepository
    {
        private readonly FileDbContext _dbContext;
        private readonly IConfiguration _configuration;

        public ConfigRepository(FileDbContext dbContext, IConfiguration configureation)
        {
            _dbContext = dbContext;
            _configuration = configureation;
        }

        public async Task<List<Country>> GetCountryAsync(List<int>? countryId)
        {
            IQueryable<Country> query = _dbContext.HCPOSCountry
               .Select(config => new Country
               {
                   CountryId = config.Id,
                   CountryCode = config.CountryCode,
                   CountryName = config.CountryName,
                   CountryDescription = config.CountryDescription
               });

            if (countryId != null && countryId.Any())
            {
                query = query.Where(c => countryId.Contains(c.CountryId));
            }

            return await query.ToListAsync();
        }

        public async Task<List<Role>> GetRolesAsync()
        {
            return await _dbContext.Roles
                .Select(role => new Role
                {
                    RoleId = role.RoleId,
                    RoleName = role.RoleName
                })
                .ToListAsync();
        }

        public async Task<List<RetailerModel>> GetRetailersAsync()
        {
            return await _dbContext.HCPOSRetailer.Select(Retailer => new RetailerModel
            {
                RetailerName = Retailer.RetailerName,
                Id = Retailer.Id
            })
                .ToListAsync() ;
                
        }
    }
}
