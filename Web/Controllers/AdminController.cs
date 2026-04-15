using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MadeByMe.Application.Services.Interfaces;

namespace MadeByMe.src.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }


        public async Task<IActionResult> Index(string searchTerm)
        {
            var users = await _adminService.GetUsersAsync();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                users = users.Where(u => 
                    (u.UserName != null && u.UserName.ToLower().Contains(searchTerm)) || 
                    (u.Email != null && u.Email.ToLower().Contains(searchTerm))
                ).ToList();
            }

            ViewData["CurrentFilter"] = searchTerm; 
            return View(users);
        }

        public async Task<IActionResult> Block(string id)
        {
            await _adminService.BlockUserAsync(id);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Unblock(string id)
        {
            await _adminService.UnblockUserAsync(id);
            return RedirectToAction("Index");
        }
    }
}
