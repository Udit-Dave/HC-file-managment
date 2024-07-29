
using HCFileCorrection.Entities;

namespace HCFileCorrection.Models
{
    public class AuthenticationResult
    {
        public int UserId { get; set; }
        public string RoleName { get; set; }

        public string UserName { get; set; }
        public string Token { get; set; }

        public List<DTCountry> UserCountry { get; set; }

       
    }

}
