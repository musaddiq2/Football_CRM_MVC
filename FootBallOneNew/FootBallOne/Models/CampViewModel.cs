using System.ComponentModel.DataAnnotations;

namespace FootBallOne.Models
{
    public class CampViewModel
    {
        public int CampID { get; set; }

        [Required(ErrorMessage = "Camp name is required")]
        [Display(Name = "Camp Name")]
        public string CampName { get; set; }

        [Display(Name = "Camp Description")]
        public string CampDescription { get; set; }

        [Required(ErrorMessage = "Start date is required")]
        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End date is required")]
        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "Branch is required")]
        [Display(Name = "Branch")]
        public string Branch { get; set; }

        [Display(Name = "Capacity")]
        [Range(1, int.MaxValue, ErrorMessage = "Capacity must be at least 1")]
        public int? Capacity { get; set; }

        [Required(ErrorMessage = "Ticket price is required")]
        [Display(Name = "Ticket Price")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Ticket price must be greater than 0")]
        public decimal TicketPrice { get; set; }

        [Display(Name = "Address")]
        public string Address { get; set; }

        [Display(Name = "City")]
        public string City { get; set; }

        [Display(Name = "Latitude")]
        public double? Latitude { get; set; }

        [Display(Name = "Longitude")]
        public double? Longitude { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } // ✅ Add this line

        public List<CampActivity> Activities { get; set; } = new List<CampActivity>();

        public List<CampInvitedFacility> InvitedFacilities { get; set; } = new List<CampInvitedFacility>();
    }


    public class CampActivity
    {
        public int ActivityID { get; set; }  // Changed to match your database column
        public int CampID { get; set; }

        [Display(Name = "Activity Name")]
        public string ActivityName { get; set; }

        [Display(Name = "Activity Description")]
        public string ActivityDescription { get; set; }
    }

    public class CampInvitedFacility
    {
        public int InvitedFacilityID { get; set; }
        public int CampID { get; set; }

        [Required(ErrorMessage = "Facility name is required")]
        [Display(Name = "Facility Name")]
        public string FacilityName { get; set; }

        [Display(Name = "Invitation Message")]
        public string InvitationMessage { get; set; }
    }

    // Main Camp entity (this should match your database model)
    public class Camp : IAcademyEntity
    {
        public int CampID { get; set; }

        public int? AcademyID { get; set; }
        public string CampName { get; set; }
        public string CampDescription { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Branch { get; set; }
        public int? Capacity { get; set; }
        public decimal TicketPrice { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}