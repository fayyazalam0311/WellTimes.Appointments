using AppointmentService.Models;
using Microsoft.EntityFrameworkCore;

namespace AppointmentService.Data
{
    public class AppointmentDbContext : DbContext
    {
        public AppointmentDbContext(DbContextOptions<AppointmentDbContext> options) 
            : base(options)
        {
        }

        public DbSet<Slot> Slots => Set<Slot>();
        public DbSet<Appointment> Appointments => Set<Appointment>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Slot>(entity =>
            {
                entity.HasKey(s => s.Id);
                
                entity.Property(s => s.DoctorName)
                .IsRequired()
                .HasMaxLength(100);
                
                entity.HasIndex(s => new {s.DoctorId, s.StartTimeUtc });
            });

            modelBuilder.Entity<Appointment>(entity =>
            {
                entity.HasKey(a => a.Id);

                entity.HasOne(a => a.Slot)
                .WithMany()
                .HasForeignKey(a => a.SlotId)
                .OnDelete(DeleteBehavior.Restrict);

                entity.Property(a => a.Status)
                .HasConversion<string>()
                .HasMaxLength(100);

                entity.HasIndex(a => a.PatientId);

                entity.HasIndex(a => a.SlotId)
                .IsUnique()
                .HasFilter("[Status] = 'Booked'");
            });
        }

        protected override void ConfigureConventions(
            ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.Properties<DateTime>()
                .HaveConversion<UtcDateTimeConverter>();
            configurationBuilder.Properties<DateTime?>()
                .HaveConversion<NullableUtcDateTimeConverter>();
        }
    }
}
