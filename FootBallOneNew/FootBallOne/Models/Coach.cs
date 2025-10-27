using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FootBallOne.Models
{
    public class Coach : IAcademyEntity
    {
        [Key] // Specifies CoachId as the primary key
        public int CoachId { get; set; }
        public int? AcademyID { get; set; }

        [Required(ErrorMessage = "Full Name is required.")]
        [StringLength(100, ErrorMessage = "Full Name cannot exceed 100 characters.")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Age is required.")]
        [Range(18, 99, ErrorMessage = "Age must be between 18 and 99.")]
        public int Age { get; set; }

        [Required(ErrorMessage = "Phone Number is required.")]
        [Phone(ErrorMessage = "Invalid Phone Number format.")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone Number must be 10 digits.")]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid Email Address format.")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Specialization is required.")]
        [StringLength(50, ErrorMessage = "Specialization cannot exceed 50 characters.")]
        public string Specialization { get; set; } // e.g., Fitness, Goalkeeping, Technical

        [Required(ErrorMessage = "Experience Years is required.")]
        [Range(0, 50, ErrorMessage = "Experience Years must be between 0 and 50.")]
        [Display(Name = "Experience Years")]
        public int ExperienceYears { get; set; }

        [Required(ErrorMessage = "Working Hours are required.")]
        [StringLength(50, ErrorMessage = "Working Hours cannot exceed 50 characters.")]
        [Display(Name = "Working Hours")]
        public string WorkingHours { get; set; } // e.g., "9:00 AM - 5:00 PM" or "Full-time"

        [Display(Name = "Assigned Players")]
        [Range(0, 500, ErrorMessage = "Assigned Players must be between 0 and 500.")]
        public int AssignedPlayers { get; set; }

        [Required(ErrorMessage = "Joining Date is required.")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Joining Date")]
        public DateTime JoiningDate { get; set; }

        [Display(Name = "Photo")]
        public string? PhotoURL { get; set; } // Optional: Stores the path to the coach's photo

        [NotMapped] // This property will not be mapped to the database
        [Display(Name = "Upload Photo")]
        public IFormFile? PhotoFile { get; set; } // For file upload in forms


    }
}
