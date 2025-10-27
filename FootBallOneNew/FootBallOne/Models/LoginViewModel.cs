using System.ComponentModel.DataAnnotations;

namespace FootBallOne.Models
{
    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        // ✅ Add this
        [Required(ErrorMessage = "Please select a branch")]
        [Display(Name = "Branch")]
        public int AcademyID { get; set; }

        public bool RememberMe { get; set; }
    }
}