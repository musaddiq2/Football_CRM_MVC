using System.ComponentModel.DataAnnotations;

namespace FootBallOne.Models
{
    public class EditAcademyViewModel
    {
        public int AcademyID { get; set; }

        [Required(ErrorMessage = "Academy Name is required.")]
        public string AcademyName { get; set; }

        [Required(ErrorMessage = "Location is required.")]
        public string Location { get; set; }

        public string CompanyCode { get; set; }

        public string HeadCoach { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Capacity must be a positive number.")]
        public int? Capacity { get; set; }

        public string ContactNo { get; set; }

        public string AcademyAddress { get; set; }

        [Required(ErrorMessage = "Manager Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; }

        // Password is optional (no [Required] attribute)
        public string Password { get; set; }
    }
}