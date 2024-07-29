using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace HCFileCorrection.Entities
{


    public class UserModel
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string Username { get; set; }

        [Required]
        [StringLength(200)] // Adjust length as needed
        public string PasswordHash { get; set; }

        [Required]
        [StringLength(200)] // Adjust length as needed
        public string PasswordSalt { get; set; }

        public string? AllowedCountries { get; set; }

        public string? CreatedUser { get; set; }

        public DateTime? CreatedDateTime { get; set; }

        // Foreign key to Role
        public int RoleId { get; set; }

        // Navigation property
        [ForeignKey("RoleId")]
        public RoleModel Role { get; set; }

        public ICollection<DTUserSessionModel> Sessions { get; set; }

        public virtual ICollection<DTUserMapping> UserMappings { get; set; }
        
    }

}
