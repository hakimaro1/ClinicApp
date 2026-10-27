using ClinicApp.Data;
using ClinicApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicApp.Controllers
{
    // ВРЕМЕННО УБРАЛИ [Authorize(Roles = "Admin")], чтобы вы могли зайти
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public AdminController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var doctors = await _context.DoctorProfiles.ToListAsync();
            return View(doctors);
        }

        public async Task<IActionResult> Appointments()
        {
            var appointments = await _context.Appointments
                .Include(a => a.TimeSlot)
                    .ThenInclude(ts => ts!.Schedule)
                .Include(a => a.Doctor)
                .ToListAsync();

            var doctorProfiles = await _context.DoctorProfiles.ToListAsync();
            var users = await _userManager.Users.ToListAsync();
            var patientProfiles = await _context.PatientProfiles.ToListAsync();

            ViewBag.Users = users;
            ViewBag.DoctorProfiles = doctorProfiles;
            ViewBag.PatientProfiles = patientProfiles;

            return View(appointments);
        }

        // Подтверждение записи
        [HttpPost]
        public async Task<IActionResult> ConfirmAppointment(int appointmentId)
        {
            var appointment = await _context.Appointments
                .Include(a => a.TimeSlot)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (appointment != null && appointment.TimeSlot != null)
            {
                appointment.TimeSlot.Status = "Confirmed";
                await _context.SaveChangesAsync();
                TempData["Success"] = "Запись подтверждена!";
            }
            return RedirectToAction("Appointments");
        }

        // Отмена записи
        [HttpPost]
        public async Task<IActionResult> CancelAppointment(int appointmentId)
        {
            var appointment = await _context.Appointments
                .Include(a => a.TimeSlot)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (appointment != null && appointment.TimeSlot != null)
            {
                appointment.TimeSlot.Status = "Free";
                _context.Appointments.Remove(appointment);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Запись отменена!";
            }
            return RedirectToAction("Appointments");
        }

        [HttpGet]
        public IActionResult CreateDoctor() => View();

        [HttpPost]
        public async Task<IActionResult> CreateDoctor(DoctorCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new IdentityUser
                {
                    UserName = model.Email,
                    Email = model.Email
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Doctor");

                    var doctorProfile = new DoctorProfile
                    {
                        UserId = user.Id,
                        Specialization = model.Specialization,
                        FirstName = model.FirstName,
                        LastName = model.LastName
                    };
                    _context.DoctorProfiles.Add(doctorProfile);
                    await _context.SaveChangesAsync();

                    return RedirectToAction("Index");
                }

                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);
            }
            return View(model);
        }
    }
}