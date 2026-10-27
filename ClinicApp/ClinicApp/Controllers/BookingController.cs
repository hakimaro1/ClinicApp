using ClinicApp.Data;
using ClinicApp.Models;
using ClinicApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicApp.Controllers
{
    [Authorize]
    public class BookingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly AppointmentService _appointmentService;
        private readonly UserManager<IdentityUser> _userManager;

        public BookingController(ApplicationDbContext context, AppointmentService appointmentService, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _appointmentService = appointmentService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string specialization, int? doctorId)
        {
            var allDoctors = await _context.DoctorProfiles.ToListAsync();
            var specializations = allDoctors.Select(d => d.Specialization).Distinct().ToList();
            ViewBag.Specializations = specializations;

            var filteredDoctors = allDoctors.AsQueryable();
            if (!string.IsNullOrEmpty(specialization))
            {
                filteredDoctors = filteredDoctors.Where(d => d.Specialization == specialization);
            }
            ViewBag.Doctors = filteredDoctors.ToList();
            ViewBag.SelectedSpecialization = specialization;
            ViewBag.SelectedDoctorId = doctorId;

            var query = _context.TimeSlots
                .Include(s => s.Schedule)
                .Where(s => s.Status == "Free" && s.Start > DateTime.Now);

            if (doctorId.HasValue)
            {
                query = query.Where(s => s.Schedule!.DoctorId == doctorId.Value);
            }
            else if (!string.IsNullOrEmpty(specialization))
            {
                var docIds = filteredDoctors.Select(d => d.Id).ToList();
                query = query.Where(s => docIds.Contains(s.Schedule!.DoctorId));
            }
            else
            {
                query = query.Where(s => false); // Показываем пустоту, пока не выбран врач
            }

            var freeSlots = await query.OrderBy(s => s.Start).ToListAsync();
            ViewBag.AllDoctors = allDoctors;

            return View(freeSlots);
        }

        [HttpPost]
        public async Task<IActionResult> Book(int timeSlotId)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                TempData["Error"] = "Ошибка авторизации. Попробуйте войти заново.";
                return RedirectToAction("Index");
            }

            // Получаем слот, чтобы узнать, к какому врачу он относится
            var slot = await _context.TimeSlots
                .Include(s => s.Schedule)
                .FirstOrDefaultAsync(s => s.Id == timeSlotId);

            if (slot == null) return NotFound("Слот не найден");

            try
            {
                // Передаем DoctorId в сервис
                await _appointmentService.BookAppointmentAsync(timeSlotId, userId!, slot.Schedule!.DoctorId);
                TempData["Success"] = "Вы успешно записались!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> MyAppointments()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return RedirectToPage("/Account/Login", new { area = "Identity" });

            // Подтягиваем врача напрямую через Appointment.Doctor
            var myAppointments = await _context.Appointments
                .Include(a => a.TimeSlot)
                .Include(a => a.Doctor) // <-- ПОДТЯГИВАЕМ ВРАЧА
                .Where(a => a.PatientId == userId)
                .OrderByDescending(a => a.TimeSlot!.Start)
                .ToListAsync();

            var doctors = await _context.DoctorProfiles.ToListAsync();
            ViewBag.Doctors = doctors;

            return View(myAppointments);
        }
    }
}