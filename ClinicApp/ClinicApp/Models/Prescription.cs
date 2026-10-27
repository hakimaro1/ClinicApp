using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicApp.Models
{
    public class Prescription
    {
        public int Id { get; set; }

        public int DoctorId { get; set; }
        public virtual DoctorProfile? Doctor { get; set; }

        public string PatientId { get; set; } = string.Empty;

        public int MedicationId { get; set; }
        public virtual Medication? Medication { get; set; }

        public string Instruction { get; set; } = string.Empty;
        public int DurationDays { get; set; }

        // ИСПРАВЛЕНО: Указываем, что дата хранится без часового пояса
        [Column(TypeName = "timestamp without time zone")]
        public DateTime StartDate { get; set; }
    }
}