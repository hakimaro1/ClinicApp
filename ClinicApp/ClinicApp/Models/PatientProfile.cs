namespace ClinicApp.Models
{
    public class PatientProfile
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty; // Связь с AspNetUsers
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Patronymic { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;
    }
}
