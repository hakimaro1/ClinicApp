using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicApp.Models
{
    public class Message
    {
        public int Id { get; set; }

        // Связь с приемом (чтобы знать, к какому визиту относится переписка)
        public int AppointmentId { get; set; }
        public virtual Appointment? Appointment { get; set; }

        public string SenderId { get; set; } = string.Empty; // Кто отправил (UserId)
        public string Text { get; set; } = string.Empty;     // Текст сообщения

        [Column(TypeName = "timestamp without time zone")]
        public DateTime SentAt { get; set; } = DateTime.Now; // Дата и время
    }
}