using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicApp.Models
{
    public class MedicationSchedule
    {
        public int Id { get; set; }

        public int PrescriptionId { get; set; }
        public virtual Prescription? Prescription { get; set; }

        // ИСПРАВЛЕНО: Указываем, что время хранится без часового пояса
        [Column(TypeName = "timestamp without time zone")]
        public DateTime IntakeTime { get; set; }

        public bool IsTaken { get; set; }
    }
}