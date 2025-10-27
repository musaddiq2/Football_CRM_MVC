using System.ComponentModel.DataAnnotations;

namespace FootBallOne.Models
{
    public class SecurityAccessViewModel
    {
        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
