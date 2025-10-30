using System.ComponentModel.DataAnnotations;

namespace FootBallOne.Models
{
    public class ParentCreateViewModel
    {
        [Required]
        [Display(Name = "Full Name")]
        public string Fullname { get; set; } = string.Empty;

        [EmailAddress]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Display(Name = "Mobile Number")]
        public string? MobileNo { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string? Password { get; set; }

        [Display(Name = "Gender")]
        public string? Gender { get; set; }

        [Display(Name = "Iqama ID")]
        public string? IqamaID { get; set; }

        [Display(Name = "Photo")]
        public string? SubscriberPhoto { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Address")]
        public string? Address { get; set; }

        [Display(Name = "City")]
        public string? City { get; set; }

        [Display(Name = "Notes")]
        public string? Note { get; set; }

        [Required]
        [Display(Name = "Student")]
        public int RegistrationId { get; set; }

        // Phone 1
        [Display(Name = "Phone Number 1")]
        public string? Phone1 { get; set; }

        [Display(Name = "Label 1")]
        public string? Phone1Label { get; set; }

        [Display(Name = "WhatsApp 1")]
        public bool Phone1IsWhatsApp { get; set; }

        [Display(Name = "Notes 1")]
        public string? Phone1Notes { get; set; }

        // Phone 2
        [Display(Name = "Phone Number 2")]
        public string? Phone2 { get; set; }

        [Display(Name = "Label 2")]
        public string? Phone2Label { get; set; }

        [Display(Name = "WhatsApp 2")]
        public bool Phone2IsWhatsApp { get; set; }

        [Display(Name = "Notes 2")]
        public string? Phone2Notes { get; set; }

        // Phone 3
        [Display(Name = "Phone Number 3")]
        public string? Phone3 { get; set; }

        [Display(Name = "Label 3")]
        public string? Phone3Label { get; set; }

        [Display(Name = "WhatsApp 3")]
        public bool Phone3IsWhatsApp { get; set; }

        [Display(Name = "Notes 3")]
        public string? Phone3Notes { get; set; }

        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? LastUpdated { get; set; }
    }
}