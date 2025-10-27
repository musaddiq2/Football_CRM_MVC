using System.ComponentModel.DataAnnotations;

namespace FootBallOne.Models
{
    public class EventListViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Event Name")]
        public string EventName { get; set; }

        [Display(Name = "Date Interval")]
        public string DateInterval { get; set; }

        [Display(Name = "Available Capacity")]
        public int AvailableCapacity { get; set; }

        [Display(Name = "Available Spots")]
        public int AvailableSpots { get; set; }

        [Display(Name = "Ticket Price")]
        public decimal TicketPrice { get; set; }

        [Display(Name = "Event Type")]
        public string EventType { get; set; }

        // Additional info for display
        public string Address { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        [Display(Name = "Status")]
        public bool IsActive { get; set; }
    }

    // Filter options for the event list
    public class EventListFilterViewModel
    {
        public List<EventListViewModel> Events { get; set; }
        public string FilterType { get; set; } // "Default", "Upcoming", "Ongoing", "Competition", "Available"
        public string SearchTerm { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }

        public EventListFilterViewModel()
        {
            Events = new List<EventListViewModel>();
            CurrentPage = 1;
            PageSize = 5;
            FilterType = "Default";
        }
    }
}
