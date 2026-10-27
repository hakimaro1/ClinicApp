using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicApp.Models
{
    public class MedicalRecord
    {
        public int Id { get; set; }
        public int AppointmentId { get; set; }
        public string Diagnosis { get; set; } = string.Empty;
        public string Treatment { get; set; } = string.Empty;
        public string History { get; set; } = string.Empty;

        [Column(TypeName = "timestamp without time zone")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public virtual Appointment? Appointment { get; set; }
        public virtual ICollection<Medication> Medications { get; set; } = new List<Medication>();
    }
}