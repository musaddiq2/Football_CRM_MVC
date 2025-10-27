namespace FootBallOne.ViewModel
{
    public class InvitationViewModel
    {
        public int InvitationId { get; set; } // Adjust based on your DB model
        public string TournamentName { get; set; }
        public string InvitedTeam { get; set; }
        public DateTime InvitationDate { get; set; }
        public string Status { get; set; } // e.g., Pending, Accepted, Declined
        public string Notes { get; set; }
    }
}
