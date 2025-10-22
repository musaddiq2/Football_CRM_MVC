using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FootBallOne.Models
{
    public class RegistrationManagement
    {
        [Key]
        public int Id { get; set; }

        public int? AcademyID { get; set; }

        public string? Name { get; set; }
        public string? PhoneNo { get; set; }
        public string? Category { get; set; }
        public DateTime? SubscriptionStart { get; set; }
        public DateTime? SubscriptionEnd { get; set; }
        public decimal? SubscriptionFees { get; set; }


        [Display(Name = "Kit Fees")]
        [DataType(DataType.Currency)]
        public decimal? KitFees { get; set; }

        [Display(Name = "Sugar Fees")]
        [DataType(DataType.Currency)]
        public decimal? SugarFees { get; set; }

        [Display(Name = "Bag Fees")]
        [DataType(DataType.Currency)]
        public decimal? BagFees { get; set; }

        [DataType(DataType.Currency)]
        [Display(Name = "Total Fees")]
        public decimal TotalFees { get; set; }

        // Optional fields
        public string? Email { get; set; }
        public string? Password { get; set; }

        public string? Address { get; set; }
        public string? City { get; set; }

        [NotMapped]
        public string? ImageFile { get; set; } // path as string

        [Display(Name = "Coach Name")]
        public string? CoachName { get; set; }

        [Display(Name = "Player Working Hours")]
        public string? WorkingHours { get; set; }

        [Display(Name = "Payment Status")]
        public string? PaymentStatus { get; set; } // "Paid" or "Not Paid"

        [DataType(DataType.Date)]
        public DateTime? CreatedDate { get; set; }

        public string? Ground { get; set; }
        public string? TimeSlot { get; set; }
        public DateTime? BirthDate { get; set; }

        public DateTime? LastUpdated { get; set; } // Nullable for old data

        [NotMapped]
        public int Step { get; set; }





    }
}

