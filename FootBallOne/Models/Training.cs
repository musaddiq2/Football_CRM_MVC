using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace FootBallOne.Models
{
    public class Training
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TrainingId { get; set; }

        [StringLength(500)]
        public string? ActivityImage { get; set; }

        [Required]
        [StringLength(255)]
        public string ActivityName { get; set; }

        [Required]
        [StringLength(100)]
        public string ActivityType { get; set; }

        public string? Description { get; set; }

        [StringLength(100)]
        public string? TrainingLevel { get; set; }

        [StringLength(50)]
        public string? AgeGroup { get; set; }

        [StringLength(100)]
        public string? Branch { get; set; }

        [StringLength(20)]
        public string? Gender { get; set; }

        public string? Facilities { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }


        public int? MaximumCapacity { get; set; }

        public int? MinimumSubscribers { get; set; }

        [StringLength(50)]
        public string? CostType { get; set; }

        [Column(TypeName = "DECIMAL(10,2)")]
        public decimal? TotalCourseCost { get; set; }

        [Column(TypeName = "DECIMAL(5,2)")]
        public decimal? ProfitMargin { get; set; }

        public string? TermsConditions { get; set; }

        public string? Trainers { get; set; }

        public bool IsActive { get; set; } = true;

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime CreatedAt { get; set; }

        // Navigation property for related CourseSchedules
        public virtual ICollection<CourseSchedule> CourseSchedules { get; set; } = new List<CourseSchedule>();
    }
    public class CourseSchedule
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CourseScheduleId { get; set; }

        [Required]
        public int TrainingId { get; set; }

        [Required]
        [StringLength(20)]
        public string Day { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public TimeSpan StartTime { get; set; }

        // Navigation property for the related Training
        [ForeignKey("TrainingId")]
        public virtual Training Training { get; set; }
    }
}
