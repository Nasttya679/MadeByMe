using MadeByMe.Application.DTOs;
using MadeByMe.Application.Services.Interfaces;
using MadeByMe.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
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
        private readonly IOrderService _orderService;
        private readonly ICommentService _commentService;
        private readonly IWebHostEnvironment _env;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IApplicationUserService applicationUserService,
            IOrderService orderService,
            ICommentService commentService,
            IWebHostEnvironment env)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _ApplicationUserService = applicationUserService;
            _orderService = orderService;
            _commentService = commentService;
            _env = env;
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

            Log.Warning("Помилка реєстрації для email {Email}: {Errors}", dto.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
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
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            var user = await _userManager.FindByEmailAsync(dto.Email!);
            if (user == null)
            {
                Log.Warning("Невдала спроба входу: користувача з email {Email} не знайдено", dto.Email);
                AddErrorToModelState("Невірна електронна пошта або пароль");
                return View(dto);
            }

            if (user.IsBlocked)
            {
                Log.Warning("Спроба входу заблокованого користувача: {Email}", dto.Email);
                ModelState.AddModelError(string.Empty, "Account is blocked");
                return View(dto);
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password!, false);
            if (result.Succeeded)
            {
                Log.Information("Користувач {Email} успішно увійшов у систему", dto.Email);
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }

            Log.Warning("Невдала спроба входу для email {Email}: невірний пароль", dto.Email);
            AddErrorToModelState("Невірна електронна пошта або пароль");
            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            var userId = CurrentUserId;
            await _signInManager.SignOutAsync();
            Log.Information("Користувач {UserId} вийшов із системи", userId);
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

            if (User.IsInRole("Seller"))
            {
                var ordersResult = await _orderService.GetSellerOrdersAsync(user.Id);
                ViewBag.OrdersCount = ordersResult.IsSuccess ? ordersResult.Value.Count() : 0;

                var reviewsResult = await _commentService.GetSellerReviewsCountAsync(user.Id);
                ViewBag.ReviewsCount = reviewsResult.IsSuccess ? reviewsResult.Value : 0;
            }
            else
            {
                ViewBag.OrdersCount = 0;
                var reviewsResult = await _commentService.GetUserReviewsCountAsync(user.Id);
                ViewBag.ReviewsCount = reviewsResult.IsSuccess ? reviewsResult.Value : 0;
            }

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BecomeSeller()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Contains("Seller"))
            {
                await _userManager.AddToRoleAsync(user, "Seller");
                Log.Information("Користувач {UserId} успішно отримав роль продавця (Seller)", user.Id);
            }

            await _signInManager.RefreshSignInAsync(user);
            return RedirectToAction("Profile", "Account");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> UpdateProfile(UpdateProfileDto dto)
        {
            if (dto.ProfilePictureFile != null && dto.ProfilePictureFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(_env.WebRootPath, "images", "avatars");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + dto.ProfilePictureFile.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.ProfilePictureFile.CopyToAsync(fileStream);
                }

                dto.ProfilePicture = "/images/avatars/" + uniqueFileName;
            }

            var result = await _ApplicationUserService.UpdateUserAsync(CurrentUserId!, dto);

            if (result.IsFailure)
            {
                SetErrorMessage(result.ErrorMessage);
                return RedirectToAction("Profile");
            }

            SetSuccessMessage("Профіль успішно оновлено!");

            var user = result.Value;
            await _signInManager.RefreshSignInAsync(user);

            return RedirectToAction("Profile");
        }

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}