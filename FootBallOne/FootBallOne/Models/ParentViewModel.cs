using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace FootBallOne.Models
{
    public class ParentViewModel
    {
        [Required(ErrorMessage = "Please select a student")]
        [Display(Name = "Student")]
        public int RegistrationId { get; set; }

        [Required(ErrorMessage = "Full name is required")]
        [Display(Name = "Full Name")]
        [StringLength(100, ErrorMessage = "Name cannot be longer than 100 characters")]
        public string Fullname { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Mobile number is required")]
        [Display(Name = "Mobile Number")]
        [RegularExpression(@"^\+?[0-9]{10,15}$", ErrorMessage = "Please enter a valid mobile number")]
        public string MobileNo { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        public string? Password { get; set; }

        public string? Gender { get; set; }

        [Display(Name = "Iqama ID")]
        public string? IqamaID { get; set; }

        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Note { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        // Phone numbers collection
        public List<PhoneNumberViewModel> PhoneNumbers { get; set; } = new List<PhoneNumberViewModel>();
    }

    public class PhoneNumberViewModel
    {
        [Required(ErrorMessage = "Phone number is required")]
        [Display(Name = "Phone Number")]
        public string PhoneNo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Label is required")]
        [Display(Name = "Label")]
        public string Label { get; set; } = "Mobile";

        [Display(Name = "WhatsApp")]
        public bool IsWhatsApp { get; set; }

        [Display(Name = "Primary")]
        public bool IsPrimary { get; set; }

        [Display(Name = "Notes")]
        public string? Notes { get; set; }
    }
}