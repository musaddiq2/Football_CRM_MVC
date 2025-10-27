using System.ComponentModel.DataAnnotations;

namespace FootBallOne.Models
{
    public class Login
    {
        public int Id { get; set; }
        public int? AcademyID { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public int Age { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [Phone]
        public string PhoneNo { get; set; } 
    }
}
