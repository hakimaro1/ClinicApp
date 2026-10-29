using ClinicApp.Data;
using ClinicApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicApp.Controllers
{
    [Authorize(Roles = "Doctor")]
    public class DoctorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public DoctorController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var doctor = await _context.DoctorProfiles.FirstOrDefaultAsync(d => d.UserId == userId);
            if (doctor == null) return NotFound("Профиль врача не найден.");

            var slots = await _context.TimeSlots
                .Include(s => s.Schedule)
                .Where(s => s.Schedule!.DoctorId == doctor.Id && s.Start > DateTime.Now)
                .OrderBy(s => s.Start)
                .ToListAsync();

            return View(slots);
        }

        [HttpGet]
        public IActionResult CreateSchedule() => View();

        [HttpPost]
        public async Task<IActionResult> CreateSchedule(DateTime workDay, TimeSpan startTime, TimeSpan endTime, int slotDurationMinutes)
        {
            var userId = _userManager.GetUserId(User);
            var doctor = await _context.DoctorProfiles.FirstOrDefaultAsync(d => d.UserId == userId);
            if (doctor == null) return NotFound();

            var schedule = new Schedule
            {
                DoctorId = doctor.Id,
                WorkDay = workDay,
                StartTime = startTime,
                EndTime = endTime
            };
            _context.Schedules.Add(schedule);
            await _context.SaveChangesAsync();

            var currentTime = startTime;
            while (currentTime < endTime)
            {
                var slotEnd = currentTime.Add(TimeSpan.FromMinutes(slotDurationMinutes));
                var slot = new TimeSlot
                {
                    ScheduleId = schedule.Id,
                    Start = workDay.Date + currentTime,
                    End = workDay.Date + slotEnd,
                    Status = "Free"
                };
                _context.TimeSlots.Add(slot);
                currentTime = slotEnd;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> MyPatients()
        {
            var userId = _userManager.GetUserId(User);
            var doctor = await _context.DoctorProfiles.FirstOrDefaultAsync(d => d.UserId == userId);
            if (doctor == null) return NotFound("Профиль врача не найден.");

            var appointments = await _context.Appointments
                .Include(a => a.TimeSlot)
                    .ThenInclude(ts => ts!.Schedule)
                .Where(a => a.TimeSlot!.Schedule!.DoctorId == doctor.Id)
                .OrderByDescending(a => a.TimeSlot!.Start)
                .ToListAsync();

            var patientProfiles = await _context.PatientProfiles.ToListAsync();
            ViewBag.PatientProfiles = patientProfiles;

            return View(appointments);
        }

        [HttpGet]
        public async Task<IActionResult> MedicalRecord(int appointmentId)
        {
            var appointment = await _context.Appointments
                .Include(a => a.TimeSlot)
                    .ThenInclude(ts => ts!.Schedule)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (appointment == null) return NotFound("Прием не найден");

            var record = await _context.MedicalRecords
                .Include(r => r.Medications)
                .FirstOrDefaultAsync(r => r.AppointmentId == appointmentId);

            if (record == null)
            {
                record = new MedicalRecord { AppointmentId = appointmentId };
            }

            var patientProfile = await _context.PatientProfiles.FirstOrDefaultAsync(p => p.UserId == appointment.PatientId);
            ViewBag.PatientName = $"{patientProfile?.LastName} {patientProfile?.FirstName} {patientProfile?.Patronymic}".Trim();
            if (string.IsNullOrWhiteSpace(ViewBag.PatientName)) ViewBag.PatientName = "Неизвестный пациент";

            return View(record);
        }

        [HttpPost]
        public async Task<IActionResult> MedicalRecord(MedicalRecord model)
        {
            var record = await _context.MedicalRecords.FirstOrDefaultAsync(r => r.AppointmentId == model.AppointmentId);

            if (record == null)
            {
                _context.MedicalRecords.Add(model);
            }
            else
            {
                record.Diagnosis = model.Diagnosis;
                record.Treatment = model.Treatment;
                record.History = model.History;
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Медицинская карта сохранена!";
            return RedirectToAction("MedicalRecord", new { appointmentId = model.AppointmentId });
        }

        // Показываем только справочник (без дублей из карт)
        public async Task<IActionResult> Medications()
        {
            var meds = await _context.Medications
                .Where(m => m.MedicalRecordId == null)
                .ToListAsync();
            return View(meds);
        }

        [HttpGet]
        public IActionResult CreateMedication() => View();

        // ИСПРАВЛЕНО: Проверка на дубликаты перед добавлением
        [HttpPost]
        public async Task<IActionResult> CreateMedication(Medication model)
        {
            if (ModelState.IsValid)
            {
                // Проверяем, есть ли уже такое лекарство в справочнике
                var existingMed = await _context.Medications
                    .FirstOrDefaultAsync(m => m.MedicalRecordId == null &&
                                              m.Name.ToLower() == model.Name.ToLower() &&
                                              m.Dosage.ToLower() == model.Dosage.ToLower() &&
                                              m.Form.ToLower() == model.Form.ToLower());

                if (existingMed != null)
                {
                    // Если такое есть, показываем ошибку и возвращаем форму
                    ModelState.AddModelError("", "Такое лекарство уже есть в справочнике!");
                    return View(model);
                }

                _context.Medications.Add(model);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Лекарство успешно добавлено!";
                return RedirectToAction("Medications");
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> PrescribeMedication(int appointmentId)
        {
            var appointment = await _context.Appointments.FindAsync(appointmentId);
            if (appointment == null) return NotFound();

            // Для назначения показываем ТОЛЬКО справочник (без дублей)
            ViewBag.Medications = await _context.Medications
                .Where(m => m.MedicalRecordId == null)
                .ToListAsync();
            ViewBag.AppointmentId = appointmentId;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> PrescribeMedication(int appointmentId, int medicationId)
        {
            var appointment = await _context.Appointments.FindAsync(appointmentId);
            if (appointment == null) return NotFound();

            var record = await _context.MedicalRecords.FirstOrDefaultAsync(r => r.AppointmentId == appointmentId);

            if (record == null)
            {
                record = new MedicalRecord { AppointmentId = appointmentId };
                _context.MedicalRecords.Add(record);
                await _context.SaveChangesAsync();
            }

            var med = await _context.Medications.FindAsync(medicationId);
            if (med != null)
            {
                var newMed = new Medication
                {
                    MedicalRecordId = record.Id,
                    Name = med.Name,
                    Dosage = med.Dosage,
                    Form = med.Form,
                    IsTaken = false
                };
                _context.Medications.Add(newMed);
                await _context.SaveChangesAsync();
            }

            TempData["Success"] = "Лекарство успешно назначено!";
            return RedirectToAction("MedicalRecord", new { appointmentId = appointmentId });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveMedication(int medicationId, int appointmentId)
        {
            var med = await _context.Medications.FindAsync(medicationId);
            if (med != null && med.MedicalRecordId != null)
            {
                _context.Medications.Remove(med);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Назначение отменено!";
            }
            return RedirectToAction("MedicalRecord", new { appointmentId = appointmentId });
        }

        // ==========================================
        // ЧАТ С ПАЦИЕНТОМ (Для врача)
        // ==========================================

        public async Task<IActionResult> Chat(int appointmentId)
        {
            var userId = _userManager.GetUserId(User);
            var doctor = await _context.DoctorProfiles.FirstOrDefaultAsync(d => d.UserId == userId);
            if (doctor == null) return NotFound("Профиль врача не найден.");

            var appointment = await _context.Appointments
                .Include(a => a.TimeSlot)
                .FirstOrDefaultAsync(a => a.Id == appointmentId && a.TimeSlot!.Schedule!.DoctorId == doctor.Id);

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
    }
}