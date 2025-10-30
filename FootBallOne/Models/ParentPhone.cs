using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FootBallOne.Models
{
    public class ParentPhone
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Phone Number")]
        public string PhoneNo { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Label")]
        public string Label { get; set; } = string.Empty;

        [Display(Name = "WhatsApp")]
        public bool IsWhatsApp { get; set; }

        [Display(Name = "Notes")]
        public string? Notes { get; set; }

        [Display(Name = "Primary")]
        public bool IsPrimary { get; set; }

        // Foreign key
        public int ParentId { get; set; }

        // Navigation property
        [ForeignKey("ParentId")]
        public virtual Parent Parent { get; set; } = null!;

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}