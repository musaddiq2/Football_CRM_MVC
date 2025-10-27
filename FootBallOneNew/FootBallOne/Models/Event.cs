using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FootBallOne.Models
{
    public class Event : IAcademyEntity
    {
        public int Id { get; set; }
        public int? AcademyID { get; set; }
        // Step 1: Basic Information
        [Required]
        [StringLength(200)]
        public string Title { get; set; }
        [StringLength(500)]
        public string Description { get; set; }
        [Display(Name = "Ticket Price")]
        public decimal TicketPrice { get; set; }
        [Required]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }
        [Required]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }
        [Required]
        [StringLength(100)]
        public string Branch { get; set; }
        // Step 2: Event Details
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Capacity must be at least 1")]
        [Display(Name = "Available Capacity")]
        public int AvailableCapacity { get; set; }
        [Required]
        [StringLength(100)]
        [Display(Name = "Event Type")]
        public string EventType { get; set; }
        [Required]
        [StringLength(300)]
        [Display(Name = "Address")]
        [Column("Adress")] // Maps to DB column with original spelling
        public string Adress { get; set; }
        [Required]
        [StringLength(100)]
        public string City { get; set; }
        // Coordinates — auto-filled, no validation
        [ScaffoldColumn(false)]
        public double Latitude { get; set; }
        [ScaffoldColumn(false)]
        public double Longitude { get; set; }
        // Step 3: Invitations
        [StringLength(200)]
        public string InvitedFacility { get; set; }
        [StringLength(1000)]
        [Display(Name = "Invitation Message")]
        public string InvitationMessage { get; set; }
        // System Fields — auto-filled
        [ScaffoldColumn(false)]
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        [ScaffoldColumn(false)]
        public bool IsActive { get; set; } = true;
    }

    public class CreateEventViewModel
    {
        public int Id { get; set; }
        [ScaffoldColumn(false)]
        public int? AcademyID { get; set; }
        // Step 1: Basic Information
        [Required]
        [StringLength(200)]
        [Display(Name = "Event Name")]
        public string Title { get; set; }

        [StringLength(500)]
        [Display(Name = "Event Description")]
        public string Description { get; set; }

        [Display(Name = "Ticket Price")]
        [Range(0, double.MaxValue, ErrorMessage = "Ticket price must be positive")]
        public decimal TicketPrice { get; set; }

        [Required]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }

        [Required]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }

        [Required]
        [StringLength(100)]
        public string Branch { get; set; }

        // Step 2: Event Details
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Capacity must be at least 1")]
        [Display(Name = "Available Capacity")]
        public int AvailableCapacity { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Event Type")]
        public string EventType { get; set; }

        [Required]
        [StringLength(300)]
        [Display(Name = "Address")]
        public string Adress { get; set; }

        [Required]
        [StringLength(100)]
        public string City { get; set; }

        [Display(Name = "Latitude")]
        public float Latitude { get; set; }

        [Display(Name = "Longitude")]
        public float Longitude { get; set; }

        // Step 3: Invitations
        [StringLength(200)]
        [Display(Name = "Invited Facility")]
        public string InvitedFacility { get; set; }

        [StringLength(1000)]
        [Display(Name = "Invitation Message")]
        public string InvitationMessage { get; set; }

        // Helper Properties
        public List<string> EventTypes { get; set; }
        public List<string> Branches { get; set; }
        public bool IsActive { get; set; }

        public CreateEventViewModel()
        {
            EventTypes = new List<string> { "Events" }; //"Camps", "Matches", "Training Activities", "Tournaments"
            Branches = new List<string> { "branch" }; // Add your branches here
            StartDate = DateTime.Today;
            EndDate = DateTime.Today.AddHours(1);
            AvailableCapacity = 1;
            TicketPrice = 0;
        }
    }

    public class CalendarViewModel
    {
        public DateTime CurrentDate { get; set; }
        public List<CalendarItem> Items { get; set; }
        public string View { get; set; } = "month";
        public List<string> EventTypes { get; set; }
        public List<string> SelectedEventTypes { get; set; }

        public CalendarViewModel()
        {
            Items = new List<CalendarItem>();
            EventTypes = new List<string> { "Events", "Camps", "Matches" }; // Add Matches
            SelectedEventTypes = new List<string>();
        }
    }

    public class NavigateRequest
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string Action { get; set; }
        public string View { get; set; }
        public string[] EventTypes { get; set; }
    }
}