using OpenQA.Selenium.DevTools.V120.DOM;
using System.ComponentModel.DataAnnotations;

namespace HCFileCorrection.Models
{
    public class RegisterModel
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string Role { get; set; }

        [Required]
        public List<int> CountryId { get; set; }

        [Required]
        public string CreatedUser { get; set; }


    }
}
