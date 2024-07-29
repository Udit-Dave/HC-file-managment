using HCFileCorrection.Entities;

namespace HCFileCorrection.Interfaces
{
    public interface IAuthenticationRepository
    {
        Task<DTUserSessionModel> CreateSessionAsync(int userId);
        Task InvalidateSessionAsync(Guid sessionId);
        Task<UserModel> GetUserByUsernameAsync(string username, string password);



        IAsyncEnumerable<DTCountry> GetUserCountries(int id);


    }
}
