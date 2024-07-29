using HCFileCorrection.Entities;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HCFileCorrection.Utility
{
    public class GenerateJwtToken
    {

        public GenerateJwtToken()
        {
        }

        public string GenerateToken(UserModel user, Guid sessionId)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var keyBytes = Encoding.UTF8.GetBytes("RjA2R0FWeUZmTWZrTDQ3dWRhNjJNSFNyM2lPUWJrYWtFamw3Y3g0eGg0djE0NUVXZz0=\r\n"); 

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Role.RoleName),
                    new Claim("sessionId", sessionId.ToString())
                }),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256Signature),
                Issuer = "your_issuer",
                Audience = "your_audience_value"
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);
            return tokenString;
        }
    }
}
