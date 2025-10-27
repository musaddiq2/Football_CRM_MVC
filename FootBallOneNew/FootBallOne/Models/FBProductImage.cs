using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace FootBallOne.Models
{
    /// <summary>
    /// Product Image Entity
    /// </summary>
    [Table("FBProductImages")]
    public class FBProductImage
    {
        [Key]
        public int ImageId { get; set; }
        [Required]
        [Display(Name = "Product")]
        public int ProductId { get; set; }
        [Required(ErrorMessage = "Image URL is required")]
        [StringLength(500)]
        [Display(Name = "Image URL")]
        [DataType(DataType.ImageUrl)]
        public string ImageUrl { get; set; }
        [StringLength(200)]
        [Display(Name = "Alt Text")]
        public string AltText { get; set; }
        [Display(Name = "Primary Image")]
        public bool IsPrimary { get; set; } = false;
        [Display(Name = "Display Order")]
        public int DisplayOrder { get; set; } = 0;
        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        // Navigation Properties
        [ForeignKey("ProductId")]
        public virtual FBProduct Product { get; set; }
    }
}