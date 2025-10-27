using FootBallOne.ViewModel;

namespace FootBallOne.Models
{
    public class MatchListViewModel
    {
        public List<MatchViewModel> Matches { get; set; }
        public string SearchTerm { get; set; }
        public string SelectedTournament { get; set; }
        public string SelectedStatus { get; set; }
        public List<string> Tournaments { get; set; }
        public List<string> Statuses { get; set; }

        public MatchListViewModel()
        {
            Matches = new List<MatchViewModel>();
            Tournaments = new List<string>();
            Statuses = new List<string> { "Scheduled", "Live", "Completed", "Postponed", "Cancelled","In Progress" };
        }
    }
}
