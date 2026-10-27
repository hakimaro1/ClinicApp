using ClinicApp.Data;
using ClinicApp.Models;
using Microsoft.EntityFrameworkCore;

namespace ClinicApp.Services
{
    public class AppointmentService
    {
        private readonly ApplicationDbContext _context;

        public AppointmentService(ApplicationDbContext context)
        {
            _context = context;
        }

        // ИЗМЕНЕНО: Добавлен параметр doctorId
        public async Task<bool> BookAppointmentAsync(int timeSlotId, string patientId, int doctorId)
        {
            var slot = await _context.TimeSlots.FindAsync(timeSlotId);

            if (slot == null || slot.Status != "Free")
                throw new Exception("Слот уже занят или не существует.");

            slot.Status = "Booked";

            var appointment = new Appointment
            {
                TimeSlotId = timeSlotId,
                PatientId = patientId,
                DoctorId = doctorId // <-- СОХРАНЯЕМ ID ВРАЧА
            };

            _context.Appointments.Add(appointment);

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException)
            {
                throw new Exception("Этот слот только что занял другой пациент.");
            }
        }
    }
}