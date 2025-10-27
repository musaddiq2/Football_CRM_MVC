using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FootBallOne.Models
{
    /// <summary>
    /// Product Category Entity
    /// </summary>
    [Table("FBCategories")]
    public class FBCategory
    {
        [Key]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Category name is required")]
        [StringLength(100)]
        [Display(Name = "Category Name")]
        public string CategoryName { get; set; }

        [StringLength(500)]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "Parent Category")]
        public int? ParentCategoryId { get; set; }

        [StringLength(500)]
        [Display(Name = "Image URL")]
        public string ImageUrl { get; set; }

        [Display(Name = "Display Order")]
        public int DisplayOrder { get; set; } = 0;

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Updated At")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        [ForeignKey("ParentCategoryId")]
        public virtual FBCategory ParentCategory { get; set; }

        public virtual ICollection<FBCategory> SubCategories { get; set; }
        public virtual ICollection<FBProduct> Products { get; set; }

        public FBCategory()
        {
            SubCategories = new HashSet<FBCategory>();
            Products = new HashSet<FBProduct>();
        }
    }
}