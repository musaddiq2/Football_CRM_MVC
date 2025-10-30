using System.ComponentModel.DataAnnotations;

namespace FootBallOne.Models
{
    public class PlayerRequest
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        public string PhoneNo { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Address is required")]
        public string Address { get; set; }

        [Required(ErrorMessage = "City is required")]
        public string City { get; set; }

        public DateTime RequestedDate { get; set; } = DateTime.Now;

        public string Status { get; set; } = "Pending";

        public string? InvitationToken { get; set; }

        public int? AcademyID { get; set; }
    }
}