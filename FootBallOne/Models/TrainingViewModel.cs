using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace FootBallOne.Models
{
    public class TrainingViewModel
    {
        public int TrainingId { get; set; }
        [ScaffoldColumn(false)]
        public int? AcademyID { get; set; }

        [StringLength(500)]
        public string? ActivityImage { get; set; }

        [Required(ErrorMessage = "Activity name is required")]
        [StringLength(255)]
        public string ActivityName { get; set; }

        [Required(ErrorMessage = "Activity type is required")]
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

        [Required(ErrorMessage = "Start date is required")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End date is required")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [Range(0, 9999.99, ErrorMessage = "Duration hours cannot be negative")]
        public decimal? DurationHours { get; set; }

        [StringLength(255)]
        public string? Location { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Maximum capacity cannot be negative")]
        public int? MaximumCapacity { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Minimum subscribers cannot be negative")]
        public int? MinimumSubscribers { get; set; }

        [StringLength(50)]
        public string? CostType { get; set; }

        [Range(0, 9999999999.99, ErrorMessage = "Total course cost cannot be negative")]
        public decimal? TotalCourseCost { get; set; }

        [Range(0, 999.99, ErrorMessage = "Profit margin cannot be negative")]
        public decimal? ProfitMargin { get; set; }

        public string? TermsConditions { get; set; }

        public string? Trainers { get; set; }

        public List<CourseScheduleViewModel> CourseSchedules { get; set; } = new List<CourseScheduleViewModel>();
    }
    public class CourseScheduleViewModel
    {
        public int CourseScheduleId { get; set; }

        [Required(ErrorMessage = "Day is required")]
        [StringLength(20)]
        public string Day { get; set; }

        [Required(ErrorMessage = "Start time is required")]
        [DataType(DataType.Time)]
        public TimeSpan StartTime { get; set; }

    }
}