using HCFileCorrection.Entities;
using HCFileCorrection.Interfaces;
using HCFileCorrection.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace HCFileCorrection.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly FileDbContext _dbContext;
        private readonly IAuthenticationService _authenticationService;

        public AuthenticationController(FileDbContext dbContext,IAuthenticationService authenticationService)
        {
            _dbContext = dbContext;
            _authenticationService = authenticationService;
        }
        [Authorize(Policy = "Admin")]
        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            try
            {
                if (await _dbContext.Users.AnyAsync(u => u.Username == model.Username))
                {
                    return BadRequest("Username is already taken.");
                }

                CreatePasswordHash(model.Password, out byte[] passwordHash, out byte[] passwordSalt);
                var role = _dbContext.Roles.FirstOrDefault(r => r.RoleName.ToLower() == model.Role.ToLower());
                if (role == null)
                {
                    return BadRequest("Invalid role specified.");
                }

                var user = new UserModel
                {
                    Username = model.Username,
                    PasswordHash = Convert.ToBase64String(passwordHash), // Store hash as base64 string
                    PasswordSalt = Convert.ToBase64String(passwordSalt), // Store salt as base64 string
                    RoleId = role.RoleId,
                    CreatedDateTime = DateTime.UtcNow,
                    CreatedUser = model.CreatedUser
                };

                _dbContext.Users.Add(user);
                await _dbContext.SaveChangesAsync();

                foreach (var countryId in model.CountryId)
                {
                    var retailerIds = await _dbContext.HCPOSVendorPortalConfig
                        .Where(vp => vp.CountryId == countryId)
                        .Select(vp => vp.RetailerId)
                        .ToListAsync();

                    foreach (var retailerId in retailerIds)
                    {
                        var userMapping = new DTUserMapping
                        {
                            User_Id = user.UserId,
                            Country_Id = countryId,
                            Retailer_Id = retailerId
                        };
                        _dbContext.HCPOSUserMapping.Add(userMapping);
                    }
                }

                await _dbContext.SaveChangesAsync();

                return Ok("User registered successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }
        [HttpPost("authenticate")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginModel model)
            {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid request model.");
            }

            var token = await _authenticationService.AuthenticateAsync(model.Username, model.Password);

            if (token == null)
            {
                return Unauthorized("Invalid username or password.");
            }

            return Ok(new { Token = token });
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> LogoutAsync()
        {
            try
            {
                var sessionIdClaim = User.Claims.FirstOrDefault(c => c.Type == "sessionId");
                if (sessionIdClaim == null || !Guid.TryParse(sessionIdClaim.Value, out Guid sessionId))
                {
                    return BadRequest("Invalid session ID in token.");
                }

                
                var result = await _authenticationService.LogoutAsync(sessionId);

                if (result)
                {
                    return Ok("Logout successful.");
                }
                else
                {
                    return StatusCode(500, "Failed to logout.");
                }
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                return StatusCode(500, "An error occurred while logging out.");
            }
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<UserModel>> GetUser(int id)
        {
            var user = await _dbContext.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound("User not found");
            }
            return Ok(user);
        }
        
        
    }
}

