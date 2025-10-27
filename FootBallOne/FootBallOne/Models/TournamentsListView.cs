using System.ComponentModel.DataAnnotations;

namespace FootBallOne.Models
{
    public class TournamentsListView
    {
        public int TournamentID { get; set; }
        public string TournamentName { get; set; }
        public string Branch { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; } // Check if this is DateTime or DateTime?
        public string Status { get; set; }
        public bool IsActive { get; set; } // Added IsActive
        public List<string> InvitedFacilities { get; set; }
    }
}