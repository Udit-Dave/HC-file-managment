using HCFileCorrection.Entities;
using HCFileCorrection.Interfaces;
using HCFileCorrection.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace HCFileCorrection.Repository
{
    public class AuthenticationRepository : IAuthenticationRepository
    {
        private readonly FileDbContext _dbContext;
        private readonly IConfiguration _configuration;

        public AuthenticationRepository(FileDbContext dbContext, IConfiguration configureation)
        {
            _dbContext = dbContext;
            _configuration = configureation;
        }

        public async Task<DTUserSessionModel> CreateSessionAsync(int userId)
        {
            try
            {
                // Validate userId (e.g., positive integer check)
                if (userId <= 0)
                {
                    throw new ArgumentException("Invalid userId");
                }

                DateTime expirationDate = DateTime.Now.AddMinutes(_configuration.GetValue<int>("SessionExpirationTime"));

                // Check for existing active sessions for the user and handle accordingly (optional)
                var existingSession = await _dbContext.UserSessions.FirstOrDefaultAsync(s => s.UserId == userId && s.IsActive);
                if (existingSession != null)
                {
                    existingSession.IsActive = false;
                    existingSession.SessionEndDateTime = DateTime.UtcNow;
                }

                var session = new DTUserSessionModel
                {
                    UserId = userId,
                    SessionGuid = Guid.NewGuid(),
                    Expiration = expirationDate,
                    IsActive = true,
                    SessionCreatedDateTime = DateTime.Now
                };

                _dbContext.UserSessions.Add(session);
                await _dbContext.SaveChangesAsync();

                return session;
            }
            catch (Exception ex)
            {
                // Handle exceptions, log errors, and possibly roll back transactions
                // Example: Log.Error(ex, "Error occurred while creating user session.");
                throw; // Rethrow or handle as appropriate
            }
        }

        public async Task InvalidateSessionAsync(Guid sessionId)
        {
            try
            {
                // Validate sessionId
                if (sessionId == Guid.Empty)
                {
                    throw new ArgumentException("Invalid sessionId");
                }

                var session = await _dbContext.UserSessions.FirstOrDefaultAsync(s => s.SessionGuid == sessionId);

                if (session != null)
                {
                    session.IsActive = false;
                    session.SessionEndDateTime = DateTime.UtcNow;
                    await _dbContext.SaveChangesAsync();
                }
                else
                {
                    throw new InvalidOperationException("Session not found"); // Or handle differently based on application logic
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions, log errors, etc.
                // Example: Log.Error(ex, "Error occurred while invalidating user session.");
                throw; // Rethrow or handle as appropriate
            }
        }

        public async Task<UserModel> GetUserByUsernameAsync(string username, string password)
        {
            var user = await _dbContext.Users.Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null || !VerifyPassword(password, user.PasswordHash, user.PasswordSalt))
            {
                return null; // Return null if user is not found or password is incorrect
            }

            return user; // Return the user if authentication is successful
        }

        private bool VerifyPassword(string password, string storedHash, string storedSalt)
        {
            byte[] storedHashBytes = Convert.FromBase64String(storedHash);
            byte[] storedSaltBytes = Convert.FromBase64String(storedSalt);

            using (var hmac = new HMACSHA512(storedSaltBytes))
            {
                byte[] computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != storedHashBytes[i])
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public async IAsyncEnumerable<DTCountry> GetUserCountries(int id)
        {
            var userCountries = await _dbContext.HCPOSUserMapping
             .Where(um => um.User_Id == id)
             .Include(um => um.Country) // Ensure that the Country navigation property is included
             .Select(um => new DTCountry
             {
                 Id = um.Country.Id,
                 CountryCode = um.Country.CountryCode,
                 CountryName = um.Country.CountryName,
                 CountryDescription = um.Country.CountryDescription,
                 CreatedDate = um.Country.CreatedDate
             })
             .ToListAsync();

            foreach (var country in userCountries)
            {
                yield return country;
            }
        }
    }
}
