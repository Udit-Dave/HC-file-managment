using HCFileCorrection.Entities;
using HCFileCorrection.Interfaces;
using HCFileCorrection.Models;
using HCFileCorrection.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HCFileCorrection.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConfigController : Controller
    {
        private readonly IConfigService _configService;

        public ConfigController(IConfigService configService)
        {
            _configService = configService;
        }

        [Authorize]
        [HttpGet("GetCountryDetails")]
        public async Task<ActionResult<List<Country>>> GetCountries([FromQuery] List<int>? countryId)
        {
            try
            {
                var countries = await _configService.GetCountriesAsync(countryId);
                return Ok(countries);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [Authorize]
        [HttpGet("GetRoleDetails")]
        public async Task<ActionResult<List<RoleModel>>> GetRoles()
        {
            try
            {
                var roles = await _configService.GetRolesAsync();
                return Ok(roles);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [Authorize]
        [HttpGet("GetRetailers")]
        public async Task<ActionResult<List<RetailerModel>>> GetRetailers()
        {
            try
            {
                var retailers = await _configService.GetRetailers();
                return Ok(retailers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }

        }
    }
}
