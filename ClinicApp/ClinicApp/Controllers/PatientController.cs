using ClinicApp.Data;
using ClinicApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ClinicApp.Controllers
{
    [Authorize]
    public class PatientController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public PatientController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ==========================================
        // ЛИЧНЫЙ КАБИНЕТ (как было)
        // ==========================================

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var profile = await _context.PatientProfiles.FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null)
            {
                profile = new PatientProfile { UserId = userId! };
            }

            return View(profile);
        }

        [HttpPost]
        public async Task<IActionResult> Index(PatientProfile model)
        {
            var userId = _userManager.GetUserId(User);
            var profile = await _context.PatientProfiles.FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null)
            {
                model.UserId = userId!;
                _context.PatientProfiles.Add(model);
            }
            else
            {
                profile.FirstName = model.FirstName;
                profile.LastName = model.LastName;
                profile.Patronymic = model.Patronymic;
                profile.PhoneNumber = model.PhoneNumber;
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Ваши данные успешно сохранены!";
            return RedirectToAction("Index");
        }

        // ==========================================
        // ИСТОРИЯ БОЛЕЗНИ (как было)
        // ==========================================

        public async Task<IActionResult> MedicalHistory()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return RedirectToPage("/Account/Login", new { area = "Identity" });

            var records = await _context.MedicalRecords
                .Include(m => m.Appointment)
                    .ThenInclude(a => a!.Doctor)
                .Include(m => m.Medications)
                .Where(m => m.Appointment!.PatientId == userId)
                .OrderByDescending(m => m.Appointment!.TimeSlot!.Start)
                .ToListAsync();

            return View(records);
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsTaken(int medicationId)
        {
            var med = await _context.Medications.FindAsync(medicationId);
            if (med != null)
            {
                med.IsTaken = true;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Отмечено, что лекарство принято!";
            }
            return RedirectToAction("MedicalHistory");
        }

        [HttpPost]
        public async Task<IActionResult> CancelMedication(int medicationId)
        {
            var med = await _context.Medications.FindAsync(medicationId);
            if (med != null)
            {
                _context.Medications.Remove(med);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Лекарство отменено.";
            }
            return RedirectToAction("MedicalHistory");
        }

        // ==========================================
        // ЧАТ С ВРАЧОМ (как было)
        // ==========================================

        public async Task<IActionResult> Chat(int appointmentId)
        {
            var userId = _userManager.GetUserId(User);

            var appointment = await _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.TimeSlot)
                .FirstOrDefaultAsync(a => a.Id == appointmentId && a.PatientId == userId);

            if (appointment == null) return NotFound("Прием не найден.");

            var messages = await _context.Messages
                .Where(m => m.AppointmentId == appointmentId)
                .OrderBy(m => m.SentAt)
                .ToListAsync();

            ViewBag.Appointment = appointment;
            return View(messages);
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage(int appointmentId, string text)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(text)) return RedirectToAction("Chat", new { appointmentId });

            var message = new Message
            {
                AppointmentId = appointmentId,
                SenderId = userId!,
                Text = text,
                SentAt = DateTime.Now
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            return RedirectToAction("Chat", new { appointmentId });
        }

        // ==========================================
        // НОВОЕ: СВОИ ЗАПИСИ И ОТМЕНА
        // ==========================================

        // Список своих записей
        public async Task<IActionResult> MyAppointments()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var appointments = await _context.Appointments
                .Include(a => a.TimeSlot)
                    .ThenInclude(ts => ts.Schedule)
                .Include(a => a.Doctor)
                .Where(a => a.PatientId == userId)
                .OrderBy(a => a.TimeSlot!.Start)   // у Appointment нет AppointmentTime, время лежит в TimeSlot
                .ToListAsync();

            return View(appointments);
        }

        // Отмена своей записи
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelMyAppointment(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == id && a.PatientId == userId);

            if (appointment == null) return Forbid(); // не его запись

            if (appointment.Status == "Cancelled")
                return RedirectToAction(nameof(MyAppointments)); // уже отменена

            appointment.Status = "Cancelled";
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(MyAppointments));
        }
    }
}