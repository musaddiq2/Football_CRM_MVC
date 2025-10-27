
using FootBallOne.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FootBallOne.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // =============================================
        // Core Entities (Existing)
        // =============================================
        public DbSet<Login> Logintbl { get; set; }
        public DbSet<RegistrationManagement> RGManagements { get; set; }
        public DbSet<Admin> Admintbl { get; set; }
        public DbSet<Academy> FootballAcademy { get; set; }
        public DbSet<Coach> Coaches { get; set; }
        public DbSet<PlayerRequest> PlayerRequests { get; set; }
        public DbSet<CoachAttendance> CoachAttendances { get; set; }
        public DbSet<PlayerAttendance> PlayerAttendances { get; set; }

        // Camp Management
        public DbSet<Camp> Camps { get; set; }
        public DbSet<CampActivity> CampActivities { get; set; }
        public DbSet<CampInvitedFacility> CampInvitedFacilities { get; set; }

        // Events & Matches
        public DbSet<MatchInfo> MatchInfo { get; set; }
        public DbSet<Event> Events { get; set; }

        // Training & Courses
        public DbSet<Training> Trainings { get; set; }
        public DbSet<CourseSchedule> CourseSchedules { get; set; }

        // Tournaments & Teams
        public DbSet<Tournament> Tournaments { get; set; }
        public DbSet<Team> Teams { get; set; }

        // Facilities
        public DbSet<InvitedFacility> InvitedFacilities { get; set; }
        public DbSet<InvitationLink> InvitationLinks { get; set; }

        // Parents
        public DbSet<Parent> Parents { get; set; }
        public DbSet<ParentPhone> ParentPhones { get; set; }

        // =============================================
        // E-Commerce Entities (NEW)
        // =============================================
        public DbSet<FBCategory> FBCategories { get; set; }
        public DbSet<FBProduct> FBProducts { get; set; }
        public DbSet<FBProductVariant> FBProductVariants { get; set; }
        public DbSet<FBProductImage> FBProductImages { get; set; }
        public DbSet<FBProductReview> FBProductReviews { get; set; }
        public DbSet<FBShoppingCartItem> FBShoppingCartItems { get; set; }
        public DbSet<FBOrder> FBOrders { get; set; }
        public DbSet<FBOrderItem> FBOrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // =============================================
            // EXISTING CONFIGURATIONS
            // =============================================

            // Event Configuration
            modelBuilder.Entity<Event>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.StartDate).IsRequired();
                entity.Property(e => e.EndDate).IsRequired();
                entity.Property(e => e.EventType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.IsActive).HasDefaultValue(true);

                entity.HasIndex(e => e.StartDate).HasDatabaseName("IX_Events_StartDate");
                entity.HasIndex(e => e.EventType).HasDatabaseName("IX_Events_EventType");
                entity.HasIndex(e => e.IsActive).HasDatabaseName("IX_Events_IsActive");
            });

            // Camp Relationships
            modelBuilder.Entity<Camp>(entity =>
            {
                entity.HasKey(c => c.CampID);

                entity.HasMany<CampActivity>()
                      .WithOne()
                      .HasForeignKey(a => a.CampID)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany<CampInvitedFacility>()
                      .WithOne()
                      .HasForeignKey(f => f.CampID)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<CampActivity>(entity =>
            {
                entity.HasKey(a => a.ActivityID);
                entity.Property(a => a.ActivityName).HasMaxLength(200);
            });

            modelBuilder.Entity<CampInvitedFacility>(entity =>
            {
                entity.HasKey(f => f.InvitedFacilityID);
                entity.Property(f => f.FacilityName).HasMaxLength(200);
            });

            // Training Configuration
            modelBuilder.Entity<Training>(entity =>
            {
                entity.ToTable("Training");
                entity.HasKey(t => t.TrainingId);
                entity.Property(t => t.ActivityName).IsRequired().HasMaxLength(255);
                entity.Property(t => t.ActivityType).IsRequired().HasMaxLength(100);
                entity.Property(t => t.Description).HasColumnType("TEXT");
                entity.Property(t => t.TrainingLevel).HasMaxLength(100);
                entity.Property(t => t.AgeGroup).HasMaxLength(50);
                entity.Property(t => t.Branch).HasMaxLength(100);
                entity.Property(t => t.Gender).HasMaxLength(20);
                entity.Property(t => t.Facilities).HasColumnType("TEXT");
                entity.Property(t => t.StartDate).IsRequired();
                entity.Property(t => t.EndDate).IsRequired();
                entity.Property(t => t.CostType).HasMaxLength(50);
                entity.Property(t => t.TotalCourseCost).HasColumnType("DECIMAL(10,2)");
                entity.Property(t => t.ProfitMargin).HasColumnType("DECIMAL(5,2)");
                entity.Property(t => t.TermsConditions).HasColumnType("TEXT");
                entity.Property(t => t.Trainers).HasColumnType("TEXT");
                entity.Property(t => t.IsActive).HasDefaultValue(true);
                entity.Property(t => t.CreatedAt).HasDefaultValueSql("GETDATE()");
            });

            // CourseSchedule Configuration
            modelBuilder.Entity<CourseSchedule>(entity =>
            {
                entity.ToTable("CourseSchedule");
                entity.HasKey(cs => cs.CourseScheduleId);
                entity.Property(cs => cs.Day).IsRequired().HasMaxLength(20);
                entity.Property(cs => cs.StartTime).IsRequired();

                entity.HasOne(cs => cs.Training)
                      .WithMany(t => t.CourseSchedules)
                      .HasForeignKey(cs => cs.TrainingId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Team Configuration
            modelBuilder.Entity<Team>(entity =>
            {
                entity.HasKey(e => e.TeamID);
                entity.Property(e => e.TeamName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Branch).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
            });

            // InvitationLink Configuration
            modelBuilder.Entity<InvitationLink>().Property(e => e.Token).IsRequired().HasMaxLength(32);
            modelBuilder.Entity<InvitationLink>().Property(e => e.Name).IsRequired().HasMaxLength(255);
            modelBuilder.Entity<InvitationLink>().Property(e => e.Description).HasMaxLength(1000);
            modelBuilder.Entity<InvitationLink>().Property(e => e.ShareTitle).HasMaxLength(255);
            modelBuilder.Entity<InvitationLink>().Property(e => e.ShareDescription).HasMaxLength(500);
            modelBuilder.Entity<InvitationLink>().Property(e => e.ShareImage).HasMaxLength(500);

            // Parent Relationships
            modelBuilder.Entity<Parent>()
               .HasOne(p => p.Registration)
               .WithMany(r => r.Parents)
               .HasForeignKey(p => p.RegistrationId)
               .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ParentPhone>()
                .HasOne(pp => pp.Parent)
                .WithMany(p => p.PhoneNumbers)
                .HasForeignKey(pp => pp.ParentId)
                .OnDelete(DeleteBehavior.Cascade);

            // =============================================
            // E-COMMERCE CONFIGURATIONS (NEW)
            // =============================================

            // FBCategory - Self-referencing relationship
            modelBuilder.Entity<FBCategory>()
                .HasOne(c => c.ParentCategory)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(c => c.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FBCategory>()
                .HasIndex(c => c.CategoryName)
                .IsUnique();

            modelBuilder.Entity<FBCategory>()
                .HasIndex(c => c.IsActive);

            // FBProduct Configuration
            modelBuilder.Entity<FBProduct>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FBProduct>()
                .HasIndex(p => p.SKU)
                .IsUnique();

            modelBuilder.Entity<FBProduct>()
                .HasIndex(p => p.CategoryId);

            modelBuilder.Entity<FBProduct>()
                .HasIndex(p => p.IsActive);

            modelBuilder.Entity<FBProduct>()
                .HasIndex(p => p.IsFeatured);

            modelBuilder.Entity<FBProduct>()
                .Property(p => p.BasePrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<FBProduct>()
                .Property(p => p.DiscountPercentage)
                .HasPrecision(5, 2);

            modelBuilder.Entity<FBProduct>()
                .Property(p => p.Weight)
                .HasPrecision(10, 2);

            // FBProductVariant Configuration
            modelBuilder.Entity<FBProductVariant>()
                .HasOne(v => v.Product)
                .WithMany(p => p.ProductVariants)
                .HasForeignKey(v => v.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<FBProductVariant>()
                .HasIndex(v => v.SKU)
                .IsUnique();

            modelBuilder.Entity<FBProductVariant>()
                .HasIndex(v => v.ProductId);

            modelBuilder.Entity<FBProductVariant>()
                .Property(v => v.AdditionalPrice)
                .HasPrecision(18, 2);

            // FBProductImage Configuration
            modelBuilder.Entity<FBProductImage>()
                .HasOne(i => i.Product)
                .WithMany(p => p.ProductImages)
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<FBProductImage>()
                .HasIndex(i => i.ProductId);

            modelBuilder.Entity<FBProductImage>()
                .HasIndex(i => i.IsPrimary);

            // FBProductReview Configuration
            modelBuilder.Entity<FBProductReview>()
                .HasOne(r => r.Product)
                .WithMany(p => p.ProductReviews)
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<FBProductReview>()
                .HasIndex(r => r.ProductId);

            modelBuilder.Entity<FBProductReview>()
                .HasIndex(r => r.Id);

            modelBuilder.Entity<FBProductReview>()
                .HasIndex(r => r.IsApproved);

            // FBShoppingCartItem Configuration
            modelBuilder.Entity<FBShoppingCartItem>()
                .HasOne(s => s.Product)
                .WithMany(p => p.ShoppingCartItems)
                .HasForeignKey(s => s.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FBShoppingCartItem>()
                .HasOne(s => s.Variant)
                .WithMany(v => v.ShoppingCartItems)
                .HasForeignKey(s => s.VariantId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FBShoppingCartItem>()
                .HasIndex(s => s.Id);

            modelBuilder.Entity<FBShoppingCartItem>()
                .HasIndex(s => s.ProductId);

            // FBOrder Configuration
            modelBuilder.Entity<FBOrder>()
                .HasIndex(o => o.OrderNumber)
                .IsUnique();

            modelBuilder.Entity<FBOrder>()
                .HasIndex(o => o.Id);

            modelBuilder.Entity<FBOrder>()
                .HasIndex(o => o.OrderStatus);

            modelBuilder.Entity<FBOrder>()
                .HasIndex(o => o.PaymentStatus);

            modelBuilder.Entity<FBOrder>()
                .HasIndex(o => o.CreatedAt);

            modelBuilder.Entity<FBOrder>()
                .Property(o => o.SubTotal)
                .HasPrecision(18, 2);

            modelBuilder.Entity<FBOrder>()
                .Property(o => o.DiscountAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<FBOrder>()
                .Property(o => o.ShippingCost)
                .HasPrecision(18, 2);

            modelBuilder.Entity<FBOrder>()
                .Property(o => o.TaxAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<FBOrder>()
                .Property(o => o.TotalAmount)
                .HasPrecision(18, 2);

            // FBOrderItem Configuration
            modelBuilder.Entity<FBOrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<FBOrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FBOrderItem>()
                .HasOne(oi => oi.Variant)
                .WithMany(v => v.OrderItems)
                .HasForeignKey(oi => oi.VariantId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FBOrderItem>()
                .HasIndex(oi => oi.OrderId);

            modelBuilder.Entity<FBOrderItem>()
                .HasIndex(oi => oi.ProductId);

            modelBuilder.Entity<FBOrderItem>()
                .Property(oi => oi.UnitPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<FBOrderItem>()
                .Property(oi => oi.DiscountAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<FBOrderItem>()
                .Property(oi => oi.TotalPrice)
                .HasPrecision(18, 2);
        }

        /// <summary>
        /// Override SaveChanges to automatically update UpdatedAt timestamps
        /// </summary>
        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        /// <summary>
        /// Override SaveChangesAsync to automatically update UpdatedAt timestamps
        /// </summary>
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return base.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Automatically update UpdatedAt for modified entities
        /// </summary>
        private void UpdateTimestamps()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                // Update UpdatedAt property if it exists
                if (entry.Entity.GetType().GetProperty("UpdatedAt") != null)
                {
                    entry.Property("UpdatedAt").CurrentValue = DateTime.UtcNow;
                }
            }
        }
    }
}