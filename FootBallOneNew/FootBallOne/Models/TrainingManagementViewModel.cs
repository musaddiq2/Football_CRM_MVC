using NuGet.DependencyResolver;
using FootBallOne.Data;
namespace FootBallOne.Models
{
    public class TrainingManagementViewModel
    {
        public int TrainingId { get; set; }
        public string ActivityImage { get; set; }
        public string ActivityName { get; set; }
        public string ActivityType { get; set; }
        public string Description { get; set; }
        public string TrainingLevel { get; set; }
        public string AgeGroup { get; set; }
        public string Branch { get; set; }
        public string Gender { get; set; }
        public string Facilities { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int? MaximumCapacity { get; set; }
        public int? MinimumSubscribers { get; set; }
        public string CostType { get; set; }
        public decimal? TotalCourseCost { get; set; }
        public decimal? ProfitMargin { get; set; }
        public string TermsConditions { get; set; }
        public string Trainers { get; set; }
        public List<CourseScheduleViewModel> CourseSchedules { get; set; } = new List<CourseScheduleViewModel>();
        public bool IsActive { get; set; }
        public string Status { get; set; }
    }

}