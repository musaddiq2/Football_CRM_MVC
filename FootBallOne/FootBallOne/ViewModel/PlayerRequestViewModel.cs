using System.ComponentModel.DataAnnotations;

namespace FootBallOne.ViewModel
{
    public class PlayerRequestViewModel
    {
        [Required]
        public string Name { get; set; }

        [Required, Phone]
        public string PhoneNo { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }
        public string Password { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        public string City { get; set; }
        public string Status { get; set; }
    }
}
