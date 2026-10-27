using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicApp.Models
{
    public class Schedule
    {
        public int Id { get; set; }
        public int DoctorId { get; set; }

        [Column(TypeName = "timestamp without time zone")]
        public DateTime WorkDay { get; set; }

        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        // ВАЖНО: Добавлено навигационное свойство для связи с врачом
        public virtual DoctorProfile? DoctorProfile { get; set; }
    }
}