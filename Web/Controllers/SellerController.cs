using MadeByMe.Application.Services.Interfaces;
using MadeByMe.Application.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace MadeByMe.Web.Controllers
{
    [Authorize(Roles = "Seller")]
    public class SellerController : BaseController
    {
        private readonly IOrderService _orderService;
        private readonly IPostService _postService;
        private readonly IApplicationUserService _applicationUserService;
        private readonly IWishlistService _wishlistService;

        public SellerController(
            IOrderService orderService,
            IPostService postService,
            IApplicationUserService applicationUserService,
            IWishlistService wishlistService)
        {
            _orderService = orderService;
            _postService = postService;
            _applicationUserService = applicationUserService;
            _wishlistService = wishlistService;
        }

        public async Task<IActionResult> Orders(string status = "All", string? search = null, DateTime? date = null)
        {
            Log.Information("Продавець {SellerId} переглядає журнал замовлень: статус={Status}, пошук={Search}, дата={Date}", CurrentUserId, status, search, date);

            var result = await _orderService.GetSellerOrdersAsync(CurrentUserId!, status, search, date);

            if (result.IsFailure)
            {
                SetErrorMessage("Не вдалося завантажити журнал замовлень.");
                return RedirectToAction("Profile", "Account");
            }

            ViewBag.CurrentStatus = status;
            ViewBag.Search = search;
            ViewBag.Date = date?.ToString("yyyy-MM-dd");

            return View(result.Value);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int orderId, string newStatus)
        {
            var result = await _orderService.UpdateOrderStatusAsync(orderId, newStatus, CurrentUserId!);

            if (result.IsFailure)
            {
                Log.Warning("Помилка оновлення статусу замовлення {OrderId} продавцем {SellerId}. Причина: {Error}", orderId, CurrentUserId, result.ErrorMessage);
                SetErrorMessage(result.ErrorMessage);
            }
            else
            {
                Log.Information("Продавець {SellerId} успішно оновив статус замовлення {OrderId} на '{Status}'", CurrentUserId, orderId, newStatus);
                SetSuccessMessage($"Статус замовлення №{orderId} оновлено на '{newStatus}'");
            }

            return RedirectToAction("Orders");
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Storefront(string id, string? searchTerm)
        {
            Log.Information("Користувач {CurrentUserId} переглядає вітрину продавця {SellerId}", CurrentUserId ?? "Гість", id);

            var userResult = await _applicationUserService.GetSellerProfileAsync(id);
            if (userResult.IsFailure)
            {
                SetErrorMessage(userResult.ErrorMessage);
                return RedirectToAction("Index", "Home");
            }

            var postsResult = await _postService.GetPostsBySellerIdAsync(id, searchTerm);
            var rawPosts = postsResult.IsSuccess ? postsResult.Value : new List<Domain.Entities.Post>();

            var favoritePostIds = new HashSet<int>();
            if (CurrentUserId != null && CurrentUserId != id)
            {
                var wishlistResult = await _wishlistService.GetUserWishlistAsync(CurrentUserId);
                if (wishlistResult.IsSuccess)
                {
                    favoritePostIds = wishlistResult.Value.Select(w => w.PostId).ToHashSet();
                }
            }

            var postsDto = rawPosts.Select(post => new MadeByMe.Application.DTOs.PostResponseDto
            {
                Id = post.Id,
                Title = post.Title,
                Description = post.Description,
                Price = post.Price,
                PhotoUrl = post.Photos.FirstOrDefault()?.FilePath ?? "/images/default.jpg",
                IsFavorite = favoritePostIds.Contains(post.Id),
            }).ToList();

            var vm = new StorefrontViewModel
            {
                SellerId = userResult.Value.Id,
                SellerName = userResult.Value.UserName,
                SellerAvatar = userResult.Value.ProfilePicture,
                SellerDescription = userResult.Value.SellerDescription,
                Posts = postsDto,
            };

            ViewBag.CurrentSearch = searchTerm;
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditDescription(UpdateDescriptionDto dto)
        {
            var result = await _applicationUserService.UpdateSellerDescriptionAsync(CurrentUserId!, dto.Description);

            if (result.IsFailure)
            {
                Log.Warning("Помилка оновлення опису продавцем {SellerId}. Причина: {Error}", CurrentUserId, result.ErrorMessage);
                SetErrorMessage(result.ErrorMessage);
            }
            else
            {
                SetSuccessMessage("Опис вашої вітрини успішно оновлено!");
            }

            return RedirectToAction("Storefront", new { id = CurrentUserId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveReturn(int orderId)
        {
            var result = await _orderService.ApproveReturnAsync(orderId, CurrentUserId!);
            if (result.IsFailure)
            {
                SetErrorMessage(result.ErrorMessage);
            }
            else
            {
                SetSuccessMessage("Повернення коштів успішно схвалено!");
            }

            return RedirectToAction("Orders");
        }
    }
}