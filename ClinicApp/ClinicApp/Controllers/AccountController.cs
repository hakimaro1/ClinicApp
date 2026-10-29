using ClinicApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ClinicApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // ==========================================
        // РЕГИСТРАЦИЯ ПАЦИЕНТА (Только Email и Пароль)
        // ==========================================
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
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
                    // Даем роль Пациент
                    await _userManager.AddToRoleAsync(user, "Patient");
                    // Входим в систему
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View(model);
        }

        // ==========================================
        // ВХОД ДЛЯ СОТРУДНИКОВ (Врачи и Админы)
        // ==========================================

        [HttpGet]
        public IActionResult StaffLogin()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> StaffLogin(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                var result = await _signInManager.PasswordSignInAsync(user.UserName!, password, false, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    // Умный редирект после входа
                    if (await _userManager.IsInRoleAsync(user, "Admin"))
                        return RedirectToAction("Index", "Admin");

                    if (await _userManager.IsInRoleAsync(user, "Doctor"))
                        return RedirectToAction("Index", "Doctor");

                    // Если это пациент, но он пытается войти как сотрудник
                    TempData["Error"] = "У вас нет прав сотрудника. Войдите через главную страницу.";
                    await _signInManager.SignOutAsync(); // Разлогиниваем
                    return RedirectToAction("Index", "Home");
                }
            }
            TempData["Error"] = "Неверный Email или пароль.";
            return View();
        }
    }
}