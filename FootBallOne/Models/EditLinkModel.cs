using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace FootBallOne.Models
{
    // Separate model specifically for editing - Image is optional
    public class EditLinkModel
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Expiration period is required")]
        public string ExpirationPeriod { get; set; }

        [StringLength(100, ErrorMessage = "Share title cannot exceed 100 characters")]
        public string ShareTitle { get; set; }

        [StringLength(500, ErrorMessage = "Share description cannot exceed 500 characters")]
        public string ShareDescription { get; set; }

        // Image is OPTIONAL for edit - no validation
        public IFormFile Image { get; set; }
    }
}