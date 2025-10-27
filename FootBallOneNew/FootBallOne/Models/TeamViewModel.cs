using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace FootBallOne.ViewModel // Match namespace from controller
{
    public class TeamViewModel
    {
        public int TeamID { get; set; }
        [ScaffoldColumn(false)]
        public int? AcademyID { get; set; }// Include for display/edit

        [Required(ErrorMessage = "Team name is required.")]
        [StringLength(200, ErrorMessage = "Team name cannot exceed 200 characters.")]
        public string TeamName { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "Training activity cannot exceed 200 characters.")]
        public string? TrainingActivity { get; set; }

        [Required(ErrorMessage = "Branch is required.")]
        [StringLength(200, ErrorMessage = "Branch cannot exceed 200 characters.")]
        public string Branch { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "Stadium cannot exceed 200 characters.")]
        public string? Stadium { get; set; }

        [StringLength(200, ErrorMessage = "Coach name cannot exceed 200 characters.")]
        public string? Coach { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        [StringLength(50, ErrorMessage = "Status cannot exceed 50 characters.")]
        public string Status { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        public DateTime? FoundationDate { get; set; }

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
        public string? TeamDescription { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; }

        [Display(Name = "Team Logo")]
        public IFormFile? TeamLogo { get; set; } // For uploads in create/edit

        [StringLength(500, ErrorMessage = "Logo path cannot exceed 500 characters.")]
        public string? TeamLogoPath { get; set; } // Added for display in lists/details
    }
}