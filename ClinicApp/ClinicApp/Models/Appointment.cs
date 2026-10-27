namespace ClinicApp.Models
{
    public class Appointment
    {
        public int Id { get; set; }
        public int TimeSlotId { get; set; }
        public string PatientId { get; set; } = string.Empty;

        // ДОБАВЛЯЕМ ID ВРАЧА
        public int? DoctorId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // СТАТУС ЗАПИСИ
        // Pending — ожидает, Confirmed — подтверждено, Cancelled — отменено
        public string Status { get; set; } = "Pending";

        public virtual TimeSlot? TimeSlot { get; set; }
        public virtual DoctorProfile? Doctor { get; set; } // Связь с врачом
    }
}