using System.ComponentModel.DataAnnotations;

namespace FootBallOne.Models
{
    public class MatchInfo
    {
        [Key]
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
        [Display(Name = "Match Date")]
        public DateTime MatchDate { get; set; }
        [Required]
        [Display(Name = "Match Time")]
        public TimeSpan MatchTime { get; set; }
        [StringLength(500)]
        public string Venue { get; set; }
        [StringLength(100)]
        public string Referee { get; set; }
        public int HomeScore { get; set; } = 0; // Non-nullable, default to 0
        public int AwayScore { get; set; } = 0; // Non-nullable, default to 0
        [StringLength(100)]
        public string Winner { get; set; }
        [StringLength(50)]
        public string MatchStatus { get; set; }
        [StringLength(250)]
        public string VIPGuests { get; set; }
        [StringLength(250)]
        public string Sponsors { get; set; }
        [StringLength(500)]
        public string InvitationNotes { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}