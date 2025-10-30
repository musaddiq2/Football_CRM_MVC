using FootBallOne.Models;
using Microsoft.EntityFrameworkCore;

namespace FootBallOne.Data
{
    public class ApplicationDbContext : DbContext
    {

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
          : base(options)
        {
        }
        //public DbSet<RegistrationManagement> registrationManagements { get; set; }
        public DbSet<Login> Logintbl { get; set; }
        public DbSet<RegistrationManagement> RGManagements { get; set; }
        public DbSet<Admin> Admintbl { get; set; }

        public DbSet<Academy> FootballAcademy { get; set; }

        public DbSet<Coach> Coaches { get; set; }

        public DbSet<PlayerRequest> PlayerRequests { get; set; }

        public DbSet<CoachAttendance> CoachAttendances { get; set; }

        public DbSet<PlayerAttendance> PlayerAttendances { get; set; }

        //public DbSet<Camps2> Camps2 { get; set; }
        //public DbSet<Camps2Activity> Camps2Activities { get; set; }
        //public DbSet<Camps2InvitedFacility> Camps2InvitedFacilities { get; set; }

        //  New DbSets for Camps
        public DbSet<Camp> Camps { get; set; }
        public DbSet<CampActivity> CampActivities { get; set; }
        public DbSet<CampInvitedFacility> CampInvitedFacilities { get; set; }
        public DbSet<MatchInfo> MatchInfo { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Training> Trainings { get; set; }
        public DbSet<CourseSchedule> CourseSchedules { get; set; }
        public DbSet<Tournament> Tournaments { get; set; }
        public DbSet<InvitedFacility> InvitedFacilities { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<Parent> Parents { get; set; }
        public DbSet<ParentPhone> ParentPhones { get; set; }
        public DbSet<InvitationLink> InvitationLinks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
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
            modelBuilder.Entity<CourseSchedule>()
               .HasOne(cs => cs.Training)
               .WithMany(t => t.CourseSchedules)
               .HasForeignKey(cs => cs.TrainingId)
               .OnDelete(DeleteBehavior.Cascade); // Matches ON DELETE 
            //    // ✅ New configuration for Camp relationships
            //    modelBuilder.Entity<Camp>()
            //        .HasMany(c => c.Activities)
            //        .WithOne(a => a.Camp)
            //        .HasForeignKey(a => a.CampID)
            //        .OnDelete(DeleteBehavior.Cascade);

            //    modelBuilder.Entity<Camp>()
            //        .HasMany(c => c.InvitedFacilities)
            //        .WithOne(f => f.Camp)
            //        .HasForeignKey(f => f.CampID)
            //        .OnDelete(DeleteBehavior.Cascade);
            //    base.OnModelCreating(modelBuilder);
            // Camp
            //    modelBuilder.Entity<Camp>(entity =>
            //    {
            //        entity.HasKey(c => c.CampID);
            //        entity.HasMany(c => c.Activities)
            //              .WithOne(a => a.Camp)
            //              .HasForeignKey(a => a.CampID)
            //              .OnDelete(DeleteBehavior.Cascade);

            //        entity.HasMany(c => c.InvitedFacilities)
            //              .WithOne(f => f.Camp)
            //              .HasForeignKey(f => f.CampID)
            //              .OnDelete(DeleteBehavior.Cascade);

            //        // If the table name is different:
            //        // entity.ToTable("Camps");
            //    });

            //    // CampActivity
            //    modelBuilder.Entity<CampActivity>(entity =>
            //    {
            //        entity.HasKey(a => a.ActivityID);
            //        // entity.ToTable("CampActivities"); // if your table name is pluralized
            //    });

            //    // CampInvitedFacility
            //    modelBuilder.Entity<CampInvitedFacility>(entity =>
            //    {
            //        entity.HasKey(f => f.InvitedFacilityID);
            //        // entity.ToTable("CampInvitedFacilities");
            //    });

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

            // Configure CourseSchedule entity
            modelBuilder.Entity<CourseSchedule>(entity =>
            {
                entity.ToTable("CourseSchedule");
                entity.HasKey(cs => cs.CourseScheduleId);
                entity.Property(cs => cs.TrainingId).IsRequired();
                entity.Property(cs => cs.Day).IsRequired().HasMaxLength(20);
                entity.Property(cs => cs.StartTime).IsRequired();

                // Configure the relationship with Training
                entity.HasOne(cs => cs.Training)
                      .WithMany(t => t.CourseSchedules)
                      .HasForeignKey(cs => cs.TrainingId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
            modelBuilder.Entity<Team>(entity =>
            {
                entity.HasKey(e => e.TeamID);
                entity.Property(e => e.TeamName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Branch).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
            });
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<InvitationLink>().Property(e => e.Token).IsRequired().HasMaxLength(32);
            modelBuilder.Entity<InvitationLink>().Property(e => e.Name).IsRequired().HasMaxLength(255);
            modelBuilder.Entity<InvitationLink>().Property(e => e.Description).HasMaxLength(1000);
            modelBuilder.Entity<InvitationLink>().Property(e => e.ShareTitle).HasMaxLength(255);
            modelBuilder.Entity<InvitationLink>().Property(e => e.ShareDescription).HasMaxLength(500);
            modelBuilder.Entity<InvitationLink>().Property(e => e.ShareImage).HasMaxLength(500);

            //Parent
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

        }
    }

    }
