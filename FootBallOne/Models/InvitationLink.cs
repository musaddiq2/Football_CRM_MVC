using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FootBallOne.Models
{
    public class InvitationLink
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        [StringLength(1000)]
        public string Description { get; set; }

        [Required]
        [StringLength(32)]
        public string Token { get; set; }

        public DateTime? ExpiresAt { get; set; }

        public bool IsActive { get; set; } = true;

        public int UsageCount { get; set; } = 0;

        public int? MaxUsage { get; set; }

        [StringLength(255)]
        public string ShareTitle { get; set; }

        [StringLength(500)]
        public string ShareDescription { get; set; }

        [StringLength(500)]
        public string ShareImage { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // NEW: Academy ID for multi-tenancy
        public int? AcademyID { get; set; }

        // Computed Properties
        [NotMapped]
        public bool IsExpired => ExpiresAt.HasValue && ExpiresAt.Value < DateTime.UtcNow;

        [NotMapped]
        public string FullUrl => $"/RegistrationManagements/RequestRegistration?token={Token}";

        public string GetAbsoluteUrl(string baseUrl)
        {
            if (string.IsNullOrEmpty(baseUrl))
                return FullUrl;
            return $"{baseUrl.TrimEnd('/')}/RegistrationManagements/RequestRegistration?token={Token}";
        }

        [NotMapped]
        public int? RemainingDays
        {
            get
            {
                if (!ExpiresAt.HasValue)
                    return null;
                var days = (ExpiresAt.Value - DateTime.UtcNow).Days;
                return days > 0 ? days : 0;
            }
        }

        // Methods
        public bool CanBeUsed()
        {
            if (!IsActive)
                return false;
            if (IsExpired)
                return false;
            if (MaxUsage.HasValue && UsageCount >= MaxUsage.Value)
                return false;
            return true;
        }

        public void IncrementUsage()
        {
            UsageCount++;
            UpdatedAt = DateTime.UtcNow;
            if (MaxUsage.HasValue && UsageCount >= MaxUsage.Value)
            {
                IsActive = false;
            }
        }
    }
}