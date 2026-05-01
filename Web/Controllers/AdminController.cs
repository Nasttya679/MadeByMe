using MadeByMe.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace MadeByMe.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : BaseController
    {
        private readonly IAdminService _adminService;

        private readonly IComplaintService _complaintService;

        public AdminController(IAdminService adminService, IComplaintService complaintService)
        {
            _adminService = adminService;
            _complaintService = complaintService;
        }

        public async Task<IActionResult> Index(string searchTerm)
        {
            Log.Information("Адміністратор {AdminId} переглядає список користувачів (фільтр: '{SearchTerm}')", CurrentUserId, searchTerm ?? "немає");
            var result = await _adminService.GetUsersAsync();

            if (result.IsFailure)
            {
                Log.Warning("Помилка завантаження списку користувачів для адмінки: {Error}", result.ErrorMessage);
                SetErrorMessage("Не вдалося завантажити список користувачів.");
                return View(new List<MadeByMe.Domain.Entities.ApplicationUser>());
            }

            var users = result.Value;

            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                users = users.Where(u =>
                    (u.UserName != null && u.UserName.ToLower().Contains(searchTerm)) ||
                    (u.Email != null && u.Email.ToLower().Contains(searchTerm))).ToList();
            }

            ViewData["CurrentFilter"] = searchTerm;
            return View(users);
        }

        public async Task<IActionResult> Block(string id)
        {
            var result = await _adminService.BlockUserAsync(id);

            if (result.IsFailure)
            {
                Log.Warning("Адміністратор {AdminId} не зміг заблокувати користувача {TargetId}: {Error}", CurrentUserId, id, result.ErrorMessage);
                SetErrorMessage(result.ErrorMessage);
            }
            else
            {
                Log.Information("Адміністратор {AdminId} успішно заблокував користувача {TargetId}", CurrentUserId, id);
                SetSuccessMessage("Користувача успішно заблоковано.");
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Unblock(string id)
        {
            var result = await _adminService.UnblockUserAsync(id);

            if (result.IsFailure)
            {
                Log.Warning("Адміністратор {AdminId} не зміг розблокувати користувача {TargetId}: {Error}", CurrentUserId, id, result.ErrorMessage);
                SetErrorMessage(result.ErrorMessage);
            }
            else
            {
                Log.Information("Адміністратор {AdminId} успішно розблокував користувача {TargetId}", CurrentUserId, id);
                SetSuccessMessage("Користувача розблоковано.");
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Complaints()
        {
            var result = await _complaintService.GetPendingComplaintsAsync();
            return View(result.Value);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectComplaint(int id)
        {
            var result = await _complaintService.RejectComplaintAsync(id);

            if (result.IsSuccess)
            {
                SetSuccessMessage("Скаргу відхилено.");
            }
            else
            {
                SetErrorMessage(result.ErrorMessage);
            }

            return RedirectToAction("Complaints");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveAndPunish(int id)
        {
            var result = await _complaintService.ApproveAndPunishAsync(id);

            if (result.IsSuccess)
            {
                SetSuccessMessage("Скаргу схвалено! Каральні санкції успішно застосовано 😈");
            }
            else
            {
                SetErrorMessage(result.ErrorMessage);
            }

            return RedirectToAction("Complaints");
        }
    }
}