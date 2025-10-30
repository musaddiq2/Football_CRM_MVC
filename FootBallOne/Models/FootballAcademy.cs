using System;
using System.ComponentModel.DataAnnotations;

namespace FootBallOne.Models
{
    public class FootballAcademy
    {
        [Key]
        public int AcademyID { get; set; }

        [Required]
        [Display(Name = "Academy Name")]
        public string AcademyName { get; set; }

        [Display(Name = "Company Code")]
        public string CompanyCode { get; set; }

        public string Location { get; set; }

        [Display(Name = "Head Coach")]
        public string HeadCoach { get; set; }

        public int Capacity { get; set; }

        [Display(Name = "Contact No")]
        [Phone]
        public string ContactNo { get; set; }

        [Display(Name = "Academy Address")]
        public string AcademyAddress { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        [DataType(DataType.Password)]
        public string Password { get; set; }

        public bool IsActive { get; set; } = true;

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
