namespace ClinicApp.Models
{
    public class DoctorProfile
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;

        // НОВЫЕ ПОЛЯ:
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
    }
}