using MadeByMe.Application.DTOs;
using MadeByMe.Application.Services.Interfaces;
using MadeByMe.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace MadeByMe.Web.Controllers
{
    [Authorize]
    public class OrderController : BaseController
    {
        private readonly IOrderService _orderService;
        private readonly ICartService _cartService;

        public OrderController(IOrderService orderService, ICartService cartService)
        {
            _orderService = orderService;
            _cartService = cartService;
        }

        public async Task<IActionResult> Checkout()
        {
            var cartResult = await _cartService.GetUserCartAsync(CurrentUserId!);

            if (cartResult.IsFailure || cartResult.Value == null || !cartResult.Value.Items.Any())
            {
                SetErrorMessage("Ваш кошик порожній. Додайте товари для оформлення замовлення.");
                return RedirectToAction("Index", "Cart");
            }

            ViewBag.Cart = cartResult.Value;
            return View(new OrderDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(OrderDto dto)
        {
            if (!ModelState.IsValid)
            {
                Log.Warning("Помилка валідації форми замовлення для користувача {UserId}", CurrentUserId);
                var cartResult = await _cartService.GetUserCartAsync(CurrentUserId!);
                ViewBag.Cart = cartResult.Value;
                return View(dto);
            }

            var result = await _orderService.CreateOrderAsync(CurrentUserId!, dto);

            if (result.IsFailure)
            {
                AddErrorToModelState(result.ErrorMessage);
                var cartResult = await _cartService.GetUserCartAsync(CurrentUserId!);
                ViewBag.Cart = cartResult.Value;
                return View(dto);
            }

            SetSuccessMessage("Замовлення успішно оформлено та оплачено!");
            return RedirectToAction("Success");
        }

        public IActionResult Success() => View();
    }
}