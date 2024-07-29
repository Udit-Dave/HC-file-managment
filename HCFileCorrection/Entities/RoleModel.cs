
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace HCFileCorrection.Entities
{


    public class RoleModel
    {
        [Key]
        public int RoleId { get; set; }

        [Required]
        [StringLength(30)]
        public string RoleName { get; set; }

        // Navigation property
        public ICollection<UserModel> Users { get; set; }
    }

}
