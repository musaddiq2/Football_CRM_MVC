using NuGet.DependencyResolver;
using FootBallOne.Data;
namespace FootBallOne.Models
{
    public class TrainingListview
    {
        public int TrainingId { get; set; }
        public string ActivityImage { get; set; }
        public string ActivityName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string ActivityType { get; set; }
        public int? MaximumCapacity { get; set; }
        public decimal? TotalCourseCost { get; set; }
        public string Status { get; set; }

        public string? Trainers { get; set; }
        public string? Facilities { get; set; }

        public List<CourseScheduleViewModel> CourseSchedules { get; set; }

        public double DurationHours { get; set; }

        public void SetDurationHours(DateTime start, DateTime end)
        {
            DurationHours = (end - start).TotalHours;
        }
    }

    public class CourseScheduleViewModel2
    {
        public int CourseScheduleId { get; set; }
        public string Day { get; set; }
        public TimeSpan StartTime { get; set; }
    }
}
