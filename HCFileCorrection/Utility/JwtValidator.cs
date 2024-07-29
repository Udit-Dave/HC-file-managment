using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace HCFileCorrection.Utility
{
    public class JwtValidator : ISecurityTokenValidator
    {
        private readonly FileDbContext _dbContext;
        public JwtValidator(FileDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public bool CanValidateToken => true;

        public int MaximumTokenSizeInBytes { get; set; }

        public bool CanReadToken(string securityToken)
        {
            return true;
        }

        public ClaimsPrincipal ValidateToken(string securityToken, TokenValidationParameters validationParameters, out SecurityToken validatedToken)
        {
            var handler = new JwtSecurityTokenHandler();
            var principal = handler.ValidateToken(securityToken, validationParameters, out validatedToken);

            // Custom validation of sessionId claim
            var sessionIdClaim = principal.FindFirst("sessionId");
            if (sessionIdClaim == null)
            {
                throw new SecurityTokenValidationException("sessionId claim is missing");
            }

            // Validate session using sessionIdClaim.Value
            bool sessionIsValid = ValidateSession(int.Parse(sessionIdClaim.Value));

            if (!sessionIsValid)
            {
                throw new SecurityTokenValidationException("Session is not valid");
            }

            return principal;
        }

        private bool ValidateSession(int sessionId)
        {
          
            var session = _dbContext.UserSessions.SingleOrDefault(s => s.Session_Id == sessionId);

            if (session == null || !session.IsActive || session.Expiration < DateTime.UtcNow)
            {
                return false;
            }

            return true;
        }
    }
}
