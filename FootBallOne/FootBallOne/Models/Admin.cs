using System.ComponentModel.DataAnnotations;

namespace FootBallOne.Models
{
    public class Admin
    {
        public int Id { get; set; }

        [Required]
        public int? AcademyID { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, DataType(DataType.Password)]
        public string Password { get; set; }
        // In Models/Admin.cs, add:
        public bool IsSuperAdmin { get; set; } = false;

    }

}
