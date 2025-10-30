using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FootBallOne.Models
{
    [Table("Teams")]
    public class Team : IAcademyEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TeamID { get; set; }
        public int? AcademyID { get; set; }

        [Required]
        [StringLength(200)]
        public string TeamName { get; set; } = string.Empty;

        [StringLength(200)]
        public string? TrainingActivity { get; set; }

        [Required]
        [StringLength(200)]
        public string Branch { get; set; } = string.Empty;

        [StringLength(500)]
        public string? TeamLogoPath { get; set; }

        [StringLength(200)]
        public string? Stadium { get; set; }

        [StringLength(200)]
        public string? Coach { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = string.Empty;

        public DateTime? FoundationDate { get; set; }

        [StringLength(1000)]
        public string? TeamDescription { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        [Required]
        public DateTime CreatedDate { get; set; }

        public DateTime? ModifiedDate { get; set; }
    }
}