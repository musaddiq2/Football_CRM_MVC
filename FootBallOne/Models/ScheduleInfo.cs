using System.ComponentModel.DataAnnotations;

namespace FootBallOne.Models
{
    public class ScheduleInfo
    {
        public int CourseScheduleId { get; set; }

        [Required]
        [StringLength(20)]
        public string Day { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }
    }
}
