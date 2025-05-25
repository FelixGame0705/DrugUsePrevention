using System;
using System.IO;
using BussinessObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace DataAccessObjects
{
    public class DrugUsePreventionDBContext : DbContext
    {
        public DrugUsePreventionDBContext() { }

        public DrugUsePreventionDBContext(DbContextOptions<DrugUsePreventionDBContext> options)
            : base(options) { }

        public virtual DbSet<DashboardData> DashboardData { get; set; }
        public virtual DbSet<Consultant> Consultants { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Appointment> Appointments { get; set; }
        public virtual DbSet<Course> Courses { get; set; }
        public virtual DbSet<CourseRegistration> CourseRegistrations { get; set; }
        public virtual DbSet<UserSurveyResponse> UserSurveyResponses { get; set; }
        public virtual DbSet<ProgramParticipation> ProgramParticipations { get; set; }
        public virtual DbSet<Survey> Surveys { get; set; }
        public virtual DbSet<SurveyAnswer> SurveyAnswers { get; set; }
        public virtual DbSet<Program> Programs { get; set; }
        public virtual DbSet<UserSurveyAnswer> UserSurveyAnswers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<Appointment>()
                .HasOne(a => a.User)
                .WithMany(u => u.Appointments)
                .HasForeignKey(a => a.UserID)
                .OnDelete(DeleteBehavior.Restrict); // tránh vòng lặp xóa

            modelBuilder
                .Entity<Appointment>()
                .HasOne(a => a.Consultant)
                .WithMany()
                .HasForeignKey(a => a.ConsultantID)
                .OnDelete(DeleteBehavior.Restrict);

            base.OnModelCreating(modelBuilder);
        }

        // DbSet properties for your entities
        // public DbSet<YourEntity> YourEntities { get; set; }
    }
}
