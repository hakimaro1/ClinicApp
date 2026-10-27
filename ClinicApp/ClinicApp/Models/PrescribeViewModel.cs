using System.ComponentModel.DataAnnotations;

namespace ClinicApp.Models
{
    public class PrescribeViewModel
    {
        [Required]
        public string PatientId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Выберите лекарство")]
        public int MedicationId { get; set; }

        [Required(ErrorMessage = "Введите инструкцию (например, '2 раза в день')")]
        public string Instruction { get; set; } = string.Empty;

        [Required(ErrorMessage = "Укажите длительность (в днях)")]
        [Range(1, 365, ErrorMessage = "Длительность должна быть от 1 до 365 дней")]
        public int DurationDays { get; set; } = 5;

        [Required(ErrorMessage = "Укажите время первого приема (например, 08:00)")]
        public TimeSpan FirstIntakeTime { get; set; } = new TimeSpan(8, 0, 0);
    }
}
