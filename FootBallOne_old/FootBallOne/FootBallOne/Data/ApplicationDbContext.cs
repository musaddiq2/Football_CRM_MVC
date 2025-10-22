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







    }
}
