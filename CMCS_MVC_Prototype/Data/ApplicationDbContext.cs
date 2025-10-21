using Microsoft.EntityFrameworkCore;
using CMCS_MVC_Prototype.Models;

namespace CMCS_MVC_Prototype.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Claim> Claims { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Claim>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.LecturerName)
                    .IsRequired()
                    .HasMaxLength(100);

                // CHANGED: Remove IsRequired() from LecturerId since it's auto-generated
                entity.Property(e => e.LecturerId)
                    .HasMaxLength(20); // Removed .IsRequired()

                entity.Property(e => e.Month)
                    .IsRequired()
                    .HasMaxLength(7);

                entity.Property(e => e.HoursWorked)
                    .HasColumnType("decimal(10,2)")
                    .IsRequired();

                entity.Property(e => e.HourlyRate)
                    .HasColumnType("decimal(10,2)")
                    .IsRequired();

                // Computed column for Total
                entity.Property(e => e.Total)
                    .HasColumnType("decimal(10,2)")
                    .HasComputedColumnSql("[HoursWorked] * [HourlyRate]");

                entity.Property(e => e.Notes)
                    .HasMaxLength(500);

                entity.Property(e => e.DocumentPath)
                    .HasMaxLength(255);

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasDefaultValue("Pending");

                entity.Property(e => e.Submitted)
                    .HasDefaultValueSql("GETDATE()");

                entity.Property(e => e.ActionBy)
                    .HasMaxLength(100);
            });
        }
    }
}