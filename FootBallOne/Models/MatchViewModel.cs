using System.ComponentModel.DataAnnotations;

namespace FootBallOne.ViewModel
{
    public class MatchViewModel
    {
        public int MatchId { get; set; }
        [StringLength(100)]
        public string Tournament { get; set; }
        [Required]
        [StringLength(100)]
        public string HomeTeam { get; set; }
        [Required]
        [StringLength(100)]
        public string AwayTeam { get; set; }
        [Required]
        public DateTime MatchDate { get; set; }
        [Required]
        public TimeSpan MatchTime { get; set; }
        [StringLength(500)]
        public string Venue { get; set; }
        [StringLength(100)]
        public string Referee { get; set; }
        [Range(0, int.MaxValue)]
        public int HomeScore { get; set; } = 0; // Non-nullable, default to 0
        [Range(0, int.MaxValue)]
        public int AwayScore { get; set; } = 0; // Non-nullable, default to 0
        [StringLength(100)]
        public string Winner { get; set; }
        [Required]
        [StringLength(50)]
        public string MatchStatus { get; set; }
        [StringLength(250)]
        public string VIPGuests { get; set; }
        [StringLength(250)]
        public string Sponsors { get; set; }
        [StringLength(500)]
        public string InvitationNotes { get; set; }

        public MatchViewModel()
        {
            MatchDate = DateTime.Today;
            MatchTime = TimeSpan.FromHours(12);
            MatchStatus = "Scheduled";
        }
    }
}