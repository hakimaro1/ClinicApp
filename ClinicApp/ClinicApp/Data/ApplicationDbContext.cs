using ClinicApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ClinicApp.Data
{
    // Используем стандартный IdentityDbContext
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        // Основные таблицы
        public DbSet<DoctorProfile> DoctorProfiles { get; set; }
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<TimeSlot> TimeSlots { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<PatientProfile> PatientProfiles { get; set; }

        // НОВЫЕ ТАБЛИЦЫ ДЛЯ ЛЕЧЕНИЯ
        public DbSet<Medication> Medications { get; set; }
        public DbSet<Prescription> Prescriptions { get; set; }
        public DbSet<MedicationSchedule> MedicationSchedules { get; set; }

        // НОВЫЕ ТАБЛИЦЫ
        public DbSet<MedicalRecord> MedicalRecords { get; set; }

        // НОВАЯ ТАБЛИЦА ДЛЯ СООБЩЕНИЙ
        public DbSet<Message> Messages { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // 1. Запрет двойного бронирования на уровне БД
            builder.Entity<Appointment>()
                .HasIndex(a => a.TimeSlotId)
                .IsUnique();

            // 2. Связь профиля врача с пользователем
            builder.Entity<DoctorProfile>()
                .HasOne<IdentityUser>()
                .WithOne()
                .HasForeignKey<DoctorProfile>(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}