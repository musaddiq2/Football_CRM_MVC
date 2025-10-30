using System.ComponentModel.DataAnnotations;

namespace FootBallOne.Models
{
    public class TournamentViewModel
    {
        public int? TournamentID { get; set; }
        [Required]
        [StringLength(255)]
        public string TournamentName { get; set; }

        public string TournamentDescription { get; set; }

        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; }

        public bool OpenToEveryone { get; set; }
        public bool IsActive { get; set; } // Added IsActive
        //public bool IsActive { get; set; } = true;
        //public DateTime CreateDate { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(100)]

        // New property
        public string Branch { get; set; }

        public List<InvitedFacilityViewModel> InvitedFacilities { get; set; } = new List<InvitedFacilityViewModel>();
    }

    public class InvitedFacilityViewModel
    {
        [StringLength(255)]
        public string Invitedfacility { get; set; }

        public string InvitationMessage { get; set; }
    }
}