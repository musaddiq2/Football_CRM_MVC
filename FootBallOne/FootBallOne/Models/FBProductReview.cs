using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FootBallOne.Models
{
    /// <summary>
    /// Product Review Entity
    /// </summary>
    [Table("FBProductReviews")]
    public class FBProductReview
    {
        [Key]
        public int ReviewId { get; set; }

        [Required]
        [Display(Name = "Product")]
        public int ProductId { get; set; }

        [Required]
        [Display(Name = "Student ID")]
        public int Id { get; set; } // References your existing Student table

        [Required(ErrorMessage = "Rating is required")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        [Display(Name = "Rating")]
        public int Rating { get; set; }

        [StringLength(200)]
        [Display(Name = "Review Title")]
        public string ReviewTitle { get; set; }

        [Display(Name = "Review")]
        [DataType(DataType.MultilineText)]
        public string ReviewText { get; set; }

        [Display(Name = "Verified Purchase")]
        public bool IsVerifiedPurchase { get; set; } = false;

        [Display(Name = "Approved")]
        public bool IsApproved { get; set; } = false;

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Updated At")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Computed Properties
        [NotMapped]
        [Display(Name = "Star Rating")]
        public string StarRating => new string('★', Rating) + new string('☆', 5 - Rating);

        // Navigation Properties
        [ForeignKey("ProductId")]
        public virtual FBProduct Product { get; set; }

        // Note: No navigation to Student - managed in your existing system
    }
}