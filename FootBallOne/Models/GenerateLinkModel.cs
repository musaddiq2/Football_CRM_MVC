using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace FootBallOne.Models
{
    public class GenerateLinkModel
    {
        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        [StringLength(1000)]
        public string Description { get; set; }

        [Required]
        public string ExpirationPeriod { get; set; } // e.g., "1 Day", "1 Week"

        [StringLength(255)]
        public string ShareTitle { get; set; }

        [StringLength(500)]
        public string ShareDescription { get; set; }

        public IFormFile Image { get; set; }
    }
}