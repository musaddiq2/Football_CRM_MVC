using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FootBallOne.Models
{
    public class Parent
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Full Name")]
        public string Fullname { get; set; } = string.Empty;

        [EmailAddress]
        public string? Email { get; set; }

        [Display(Name = "Mobile Number")]
        public string? MobileNo { get; set; }

        [DataType(DataType.Password)]
        public string? Password { get; set; }

        public string? Gender { get; set; }

        [Display(Name = "Iqama ID")]
        public string? IqamaID { get; set; }

        [Display(Name = "Subscriber Photo")]
        public string? SubscriberPhoto { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        public string? Address { get; set; }
        public string? City { get; set; }

        [Column(TypeName = "decimal(10, 8)")]
        public decimal? Latitude { get; set; }

        [Column(TypeName = "decimal(11, 8)")]
        public decimal? Longitude { get; set; }

        public string? Note { get; set; }

        // Foreign key
        public int RegistrationId { get; set; }

        // Navigation properties
        [ForeignKey("RegistrationId")]
        public virtual RegistrationManagement Registration { get; set; } = null!;

        public virtual ICollection<ParentPhone> PhoneNumbers { get; set; } = new List<ParentPhone>();

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? LastUpdated { get; set; }
    }
}