using HCFileCorrection.Entities;
using HCFileCorrection.Models;

namespace HCFileCorrection.Interfaces
{
    public interface IAuthenticationService
    {
        Task<AuthenticationResult> AuthenticateAsync(string username, string password);
        Task<bool> LogoutAsync(Guid sessionId);
    }
}
