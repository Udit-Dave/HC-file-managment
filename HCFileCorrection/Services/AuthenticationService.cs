using HCFileCorrection.Entities;
using HCFileCorrection.Interfaces;
using HCFileCorrection.Models;
using HCFileCorrection.Utility;

namespace HCFileCorrection.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IAuthenticationRepository _authenticationRepository;
        private readonly IConfiguration _configuration;
        private readonly GenerateJwtToken _generateJwtToken;
        public AuthenticationService(IAuthenticationRepository authenticationRepository, IConfiguration configuration,GenerateJwtToken generateJwtToken)
        {
            _configuration = configuration;
            _authenticationRepository = authenticationRepository;
            _generateJwtToken = generateJwtToken;
        }


        public async Task<AuthenticationResult> AuthenticateAsync(string username, string password)
        {
            try
            {
                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    throw new ArgumentException("Username and password are required.");
                }

                var user = await _authenticationRepository.GetUserByUsernameAsync(username, password);

                if (user == null)
                {
                    return null;
                }

                var session = await _authenticationRepository.CreateSessionAsync(user.UserId);

                if (session == null)
                {
                    throw new InvalidOperationException("Failed to create user session.");
                }

                var tokenString = _generateJwtToken.GenerateToken(user, session.SessionGuid);

                var countries = new List<DTCountry>();
                await foreach (var country in _authenticationRepository.GetUserCountries(user.UserId))
                {
                    countries.Add(country);
                }

                if (string.IsNullOrEmpty(tokenString))
                {
                    throw new InvalidOperationException("Failed to generate JWT token.");
                }
                var result = new AuthenticationResult
                {
                    UserId = user.UserId,
                    RoleName = user.Role.RoleName,
                    UserName = user.Username,
                    UserCountry = countries,
                    Token = tokenString
                };


                return result;
            }
            catch (Exception ex)
            {
                
                throw; // Rethrow or handle as appropriate
            }
        }

        public async Task<bool> LogoutAsync(Guid sessionId)
        {
            try
            {
                await _authenticationRepository.InvalidateSessionAsync(sessionId);
                return true;
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                return false;
            }
        }


    }
}
