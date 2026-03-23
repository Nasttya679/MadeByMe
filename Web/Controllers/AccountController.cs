using MadeByMe.Application.Services;
using MadeByMe.Application.DTOs;
using MadeByMe.Application.ViewModels;
using MadeByMe.Domain.Entities;
using MadeByMe.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using MadeByMe.Application.Services.Interfaces;

namespace MadeByMe.src.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IApplicationUserService _ApplicationUserService;

        public AccountController(UserManager<ApplicationUser> userManager,
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
                return View(dto);

            var user = new ApplicationUser
            {
                UserName = dto.UserName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                Address = dto.Address
               
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                await _userManager.AddToRoleAsync(user, "User");
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

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
                return View(dto);

            var user = await _userManager.FindByEmailAsync(dto.Email);
            Console.WriteLine("User found: " + (user != null));

            if (user == null)
            {
                ModelState.AddModelError("", "Невірна електронна пошта або пароль");
                return View(dto);
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);

            Console.WriteLine("SignIn success: " + result.Succeeded);
            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }
               

            ModelState.AddModelError("", "Невірна електронна пошта або пароль");
            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return RedirectToAction(nameof(Login));

            return View(user);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return RedirectToAction(nameof(Login));

            var dto = new UpdateProfileDto
            {
                UserId = currentUser.Id,
                Email = currentUser.Email,
                UserName = currentUser.UserName,
                PhoneNumber = currentUser.PhoneNumber,
                Address = currentUser.Address               
                
            };

            return View(dto);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(UpdateProfileDto dto)
        {
            if (!ModelState.IsValid)
                return View("EditProfile", dto);

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return RedirectToAction(nameof(Login));

            dto.UserId = currentUser.Id;
            var updatedUser = _ApplicationUserService.UpdateUser(currentUser.Id, dto);

            if (updatedUser == null)
                return NotFound();

            return RedirectToAction(nameof(Profile));
        }


        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BecomeSeller()
        {
            //var user = await _userManager.GetUserAsync(User);
            //if (user == null)
            //{
            //    return RedirectToAction("Login", "Account");
            //}
            //var role_f = await _userManager.GetRolesAsync(user);
            //foreach (var role in role_f)
            //{
            //    Console.WriteLine($"Роль користувача: {role}");
            //}

            //if (!await _userManager.IsInRoleAsync(user, "Seller"))
            //{
            //    Console.WriteLine("ПОчаток циклу\n");
            //    await _userManager.AddToRoleAsync(user, "Seller");
            //    await _signInManager.RefreshSignInAsync(user);

            //    var roles = await _userManager.GetRolesAsync(user);
            //    foreach (var role in roles)
            //    {
            //        Console.WriteLine($"Роль користувача: {role}");
            //    }

            //}

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Contains("Seller"))
            {
                await _userManager.AddToRoleAsync(user, "Seller");
            }

            await _signInManager.RefreshSignInAsync(user);

            return RedirectToAction("Profile", "Account");
        }

    }

}
