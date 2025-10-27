using System.ComponentModel.DataAnnotations;

namespace FootBallOne.Models
{
    public class Admin
    {
        [Key]
        public int Id { get; set; }

        // Foreign key linking admin to an academy/branch
        [Display(Name = "Academy/Branch ID")]
        public int? AcademyID { get; set; }


        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        // Column confirmed to be in your database
        [Display(Name = "Super Admin")]
        public bool IsSuperAdmin { get; set; }


        // NOTE: The following fields were REMOVED to fix the "Invalid column name" SQL error:
        // AcademyName, CompanyCode, Location, HeadCoach, Capacity, ContactNo, AcademyAddress.
        // If these fields are needed, they should be in a separate 'FootballAcademy' model 
        // and table, not the 'Admin' table.
    }
}