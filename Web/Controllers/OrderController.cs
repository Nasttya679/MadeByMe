using MadeByMe.Application.DTOs;
using MadeByMe.Application.Services.Interfaces;
using MadeByMe.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace MadeByMe.Web.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly ICartService _cartService;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrderController(
            IOrderService orderService,
            ICartService cartService,
            UserManager<ApplicationUser> userManager)
        {
            _orderService = orderService;
            _cartService = cartService;
            _userManager = userManager;
        }

        public IActionResult Checkout()
        {
            var userId = _userManager.GetUserId(User);

            var cartResult = _cartService.GetUserCart(userId!);

            if (cartResult.IsFailure || cartResult.Value == null || !cartResult.Value.Items.Any())
            {
                TempData["Error"] = "Ваш кошик порожній. Додайте товари для оформлення замовлення.";
                return RedirectToAction("Index", "Cart");
            }

            ViewBag.Cart = cartResult.Value;

            return View(new OrderDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(OrderDto dto)
        {
            var userId = _userManager.GetUserId(User);

            if (!ModelState.IsValid)
            {
                Log.Warning("Помилка валідації форми замовлення для користувача {UserId}", userId);
                var cartResult = _cartService.GetUserCart(userId!);
                ViewBag.Cart = cartResult.Value;
                return View(dto);
            }

            var result = await _orderService.CreateOrderAsync(userId!, dto);

            if (result.IsFailure)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage);
                var cartResult = _cartService.GetUserCart(userId!);
                ViewBag.Cart = cartResult.Value;
                return View(dto);
            }

            TempData["Success"] = "Замовлення успішно оформлено та оплачено!";

            // TODO: Пізніше змінимо це на перенаправлення на сторінку "Історія замовлень"
            return RedirectToAction("Success");
        }

        public IActionResult Success()
        {
            return View();
        }
    }
}