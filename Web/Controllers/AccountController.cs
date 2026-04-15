using MadeByMe.Application.DTOs;
using MadeByMe.Application.Services.Interfaces;
using MadeByMe.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace MadeByMe.Web.Controllers
{
    public class AccountController : BaseController
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IApplicationUserService _ApplicationUserService;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IApplicationUserService applicationUserService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _ApplicationUserService = applicationUserService;
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            var user = new ApplicationUser
            {
                UserName = dto.UserName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                Address = dto.Address,
            };

            var result = await _userManager.CreateAsync(user, dto.Password!);
            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                await _userManager.AddToRoleAsync(user, "User");
                Log.Information("Користувача успішно зареєстровано: {Email}", dto.Email);
                return RedirectToAction("Index", "Home");
            }

            Log.Warning("Невдала спроба реєстрації для {Email}. Помилки: {@Errors}", dto.Email, result.Errors.Select(e => e.Description));
            foreach (var error in result.Errors)
            {
                AddErrorToModelState(error.Description);
            }

            return View(dto);
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine(error.ErrorMessage);
            }

            Console.WriteLine("ModelState.IsValid: " + ModelState.IsValid);
            Console.WriteLine("Email: " + dto.Email);
            Console.WriteLine("Password: " + dto.Password);

            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            var user = await _userManager.FindByEmailAsync(dto.Email!);
            Console.WriteLine("User found: " + (user != null));

            if (user == null)
            {
                Log.Warning("Невдала спроба входу: користувача з email {Email} не знайдено", dto.Email);
                AddErrorToModelState("Невірна електронна пошта або пароль");
                return View(dto);
            }

            if (user.IsBlocked)
            {
                ModelState.AddModelError("", "Account is blocked");
                return View(dto);
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password!, false);

            Console.WriteLine("SignIn success: " + result.Succeeded);
            if (result.Succeeded)
            {
                Log.Information("Користувач {Email} успішно увійшов у систему", dto.Email);
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }

            Log.Warning("Невдала спроба входу для {Email}: невірний пароль", dto.Email);
            AddErrorToModelState("Невірна електронна пошта або пароль");
            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            Log.Information("Користувач {UserName} вийшов із системи", CurrentUserName ?? "Невідомий");
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.FindByIdAsync(CurrentUserId!);

            if (user == null)
            {
                return RedirectToAction(nameof(Login));
            }

            return View(user);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction(nameof(Login));
            }

            var dto = new UpdateProfileDto
            {
                UserId = currentUser.Id,
                Email = currentUser.Email,
                UserName = currentUser.UserName,
                PhoneNumber = currentUser.PhoneNumber,
                Address = currentUser.Address,
            };

            return View(dto);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(UpdateProfileDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View("EditProfile", dto);
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction(nameof(Login));
            }

            dto.UserId = currentUser.Id;

            var result = await _ApplicationUserService.UpdateUserAsync(currentUser.Id, dto);

            if (result.IsFailure)
            {
                Log.Warning("Користувачу {Email} не вдалося оновити профіль. Причина: {ErrorMessage}", currentUser.Email, result.ErrorMessage);
                AddErrorToModelState(result.ErrorMessage);
                return View("EditProfile", dto);
            }

            Log.Information("Користувач {Email} успішно оновив свій профіль", currentUser.Email);
            return RedirectToAction(nameof(Profile));
        }

        public IActionResult AccessDenied()
        {
            Log.Warning("Відмовлено в доступі для користувача {UserName} до захищеного ресурсу", CurrentUserName ?? "Неавторизований");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BecomeSeller()
        {
            // var user = await _userManager.GetUserAsync(User);
            // if (user == null)
            // {
            // return RedirectToAction("Login", "Account");
            // }
            // var role_f = await _userManager.GetRolesAsync(user);
            // foreach (var role in role_f)
            // {
            // Console.WriteLine($"Роль користувача: {role}");
            // }
            // if (!await _userManager.IsInRoleAsync(user, "Seller"))
            // {
            // Console.WriteLine("ПОчаток циклу\n");
            // await _userManager.AddToRoleAsync(user, "Seller");
            // await _signInManager.RefreshSignInAsync(user);
            // var roles = await _userManager.GetRolesAsync(user);
            // foreach (var role in roles)
            // {
            // Console.WriteLine($"Роль користувача: {role}");
            // }
            // }
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Contains("Seller"))
            {
                await _userManager.AddToRoleAsync(user, "Seller");
                Log.Information("Користувач {Email} отримав роль 'Seller'", user.Email);
            }

            await _signInManager.RefreshSignInAsync(user);

            return RedirectToAction("Profile", "Account");
        }
    }
}