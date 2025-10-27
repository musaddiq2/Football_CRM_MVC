using FootBallOne.Models;
using System.ComponentModel.DataAnnotations;

public class CalendarItem
{
    public int Id { get; set; }
    [Required]
    public string Title { get; set; }
    [Required]
    public DateTime StartDate { get; set; }
    [Required]
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    public bool IsCamp { get; set; }
    public bool IsMatch { get; set; }
    public bool IsTraining { get; set; }
    public bool IsTournament { get; set; }  // <-- add this

    [StringLength(500)]
    public string Description { get; set; }

    // Match-specific properties
    [StringLength(100)]
    public string HomeTeam { get; set; }
    [StringLength(100)]
    public string AwayTeam { get; set; }
    [StringLength(100)]
    public string Tournament { get; set; }
    [StringLength(500)]
    public string Venue { get; set; }
    public int? HomeScore { get; set; }
    public int? AwayScore { get; set; }
    [StringLength(50)]
    public string MatchStatus { get; set; }
    [StringLength(100)]
    public string Referee { get; set; }
    [StringLength(250)]
    public string VIPGuests { get; set; }
    [StringLength(250)]
    public string Sponsors { get; set; }
    [StringLength(500)]
    public string InvitationNotes { get; set; }

    // Training-specific properties
    public List<ScheduleInfo> TrainingSchedules { get; set; } = new List<ScheduleInfo>();

    // Computed properties - updated for tournament support
    public string ItemType => IsMatch ? "Match" :
                              IsCamp ? "Camp" :
                              IsTraining ? "Training" :
                              IsTournament ? "Tournament" : "Event";

    public string DisplayTitle
    {
        get
        {
            if (IsMatch)
            {
                return string.IsNullOrEmpty(Tournament)
                    ? $"{HomeTeam ?? "TBD"} vs {AwayTeam ?? "TBD"}"
                    : $"{HomeTeam ?? "TBD"} vs {AwayTeam ?? "TBD"} ({Tournament})";
            }
            return Title ?? "Untitled";
        }
    }

    public string DisplayScore => IsMatch && HomeScore.HasValue && AwayScore.HasValue
        ? $"{HomeScore} - {AwayScore}"
        : IsMatch ? "vs" : string.Empty;

    public string CssClass => IsMatch ? "match-item" :
                             IsCamp ? "camp-item" :
                             IsTraining ? "training-item" :
                             IsTournament ? "tournament-item" : "event-item";

    public string DisplayStatus => IsMatch && !string.IsNullOrEmpty(MatchStatus)
        ? MatchStatus
        : IsMatch ? "Scheduled" : string.Empty;

    public string DisplayDetails => IsMatch
        ? $"Venue: {Venue ?? "TBD"}, Status: {DisplayStatus}, Referee: {Referee ?? "TBD"}"
        : Description ?? string.Empty;
}
