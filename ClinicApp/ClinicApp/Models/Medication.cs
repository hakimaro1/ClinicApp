namespace ClinicApp.Models
{
    public class Medication
    {
        public int Id { get; set; }
        public int? MedicalRecordId { get; set; }
        public string Name { get; set; } = string.Empty; // Название
        public string Dosage { get; set; } = string.Empty; // Дозировка
        public string Form { get; set; } = string.Empty; // <-- ДОБАВЛЕНО: Форма выпуска (таблетки, сироп и т.д.)
        public string Schedule { get; set; } = string.Empty; // График приема
        public bool IsTaken { get; set; } = false;

        public virtual MedicalRecord? MedicalRecord { get; set; }
    }
}