using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FootBallOne.Models
{
    public class Academy
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AcademyID { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [Display(Name = "Academy Name")]
        public string AcademyName { get; set; }

        [Display(Name = "Company Code")]
        [StringLength(10)]
        public string? CompanyCode { get; set; }

        [Display(Name = "Location")]
        [StringLength(100)]
        public string? Location { get; set; }
        [Display(Name = "Head Coach")]
        [StringLength(100)]
        public string? HeadCoach { get; set; }

        public int? Capacity { get; set; }

        [Display(Name = "Contact Number")]
        public int? ContactNo { get; set; }

        [Display(Name = "Academy Address")]
        [StringLength(100)]
        public string? AcademyAddress { get; set; }
    }
}
