using MadeByMe.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MadeByMe.Web.Controllers
{
    [Authorize]
    public class WishlistController : BaseController
    {
        private readonly IWishlistService _wishlistService;

        public WishlistController(IWishlistService wishlistService)
        {
            _wishlistService = wishlistService;
        }

        public async Task<IActionResult> Index()
        {
            var result = await _wishlistService.GetUserWishlistAsync(CurrentUserId!);

            if (result.IsFailure)
            {
                SetErrorMessage(result.ErrorMessage);
                return RedirectToAction("Index", "Home");
            }

            return View(result.Value);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleFavorite(int postId)
        {
            if (string.IsNullOrEmpty(CurrentUserId))
            {
                return Json(new { success = false, message = "Не авторизовано" });
            }

            var result = await _wishlistService.ToggleFavoriteAsync(CurrentUserId, postId);

            if (result.IsFailure)
            {
                return Json(new { success = false, message = result.ErrorMessage });
            }

            return Json(new
            {
                success = true,
                isAdded = result.Value.IsAdded,
                totalCount = result.Value.TotalCount,
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetWishlistCount()
        {
            if (!IsAuthenticated || string.IsNullOrEmpty(CurrentUserId))
            {
                return Json(new { count = 0 });
            }

            var result = await _wishlistService.GetWishlistCountAsync(CurrentUserId);

            if (result.IsFailure)
            {
                return Json(new { count = 0 });
            }

            return Json(new { count = result.Value });
        }
    }
}