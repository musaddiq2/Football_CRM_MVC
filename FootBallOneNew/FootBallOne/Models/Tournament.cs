using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace FootBallOne.Models
{

    public class Tournament : IAcademyEntity
    {
        [Key]
        public int TournamentID { get; set; }
        public int? AcademyID { get; set; }

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

        // New property
        public string Branch { get; set; }
        public bool OpenToEveryone { get; set; }
        // ✅ New field: IsActive (default true) 
        public bool IsActive { get; set; } = true;

        // ✅ New field: CreateDate (set automatically)
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;
        public ICollection<InvitedFacility> InvitedFacilities { get; set; }
    }

    public class InvitedFacility
    {
        [Key]
        public int InvitedFacilityID { get; set; }

        [ForeignKey("Tournament")]
        public int TournamentID { get; set; }

        [StringLength(255)]
        public string Invitedfacility { get; set; }

        public string InvitationMessage { get; set; }

        public Tournament Tournament { get; set; }
    }
}