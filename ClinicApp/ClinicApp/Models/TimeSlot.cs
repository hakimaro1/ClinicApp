using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicApp.Models
{
    public class TimeSlot
    {
        public int Id { get; set; }
        public int ScheduleId { get; set; }

        [Column(TypeName = "timestamp without time zone")]
        public DateTime Start { get; set; }

        [Column(TypeName = "timestamp without time zone")]
        public DateTime End { get; set; }

        public string Status { get; set; } = "Free";
        public virtual Schedule? Schedule { get; set; }
    }
}