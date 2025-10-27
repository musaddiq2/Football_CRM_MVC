namespace FootBallOne.ViewModel
{
    public class EventViewModel
    {
        public int Id { get; set; }

        // Step 1: Basic Information
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal TicketPrice { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Branch { get; set; }

        // Step 2: Event Details
        public int AvailableCapacity { get; set; }
        public string EventType { get; set; }
        public string Adress { get; set; }  // Note spelling: matches DB
        public string City { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        // Step 3: Invitations
        public string InvitedFacility { get; set; }
        public string InvitationMessage { get; set; }

        // System Fields
        public DateTime CreatedDate { get; set; }
        public bool IsActive { get; set; }
    }

}
