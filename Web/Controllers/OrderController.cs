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

        [HttpGet]
        public async Task<IActionResult> History(string status = "All", string? search = null, DateTime? date = null)
        {
            Log.Information("Користувач {UserId} переглядає журнал замовлень: статус={Status}, пошук={Search}, дата={Date}", CurrentUserId, status, search, date);

            var result = await _orderService.GetBuyerHistoryAsync(CurrentUserId!, status, search, date);

            ViewBag.CurrentStatus = status;
            ViewBag.Search = search;
            ViewBag.Date = date?.ToString("yyyy-MM-dd");

            return View(result.Value);
        }

        [HttpGet]
        public async Task<IActionResult> Cancel(int id)
        {
            var result = await _orderService.GetOrderByIdAndBuyerAsync(id, CurrentUserId!);

            if (result.IsFailure)
            {
                SetErrorMessage(result.ErrorMessage);
                return RedirectToAction("History");
            }

            if (result.Value.Status != "Processing")
            {
                SetErrorMessage("Це замовлення вже не можна скасувати (воно відправлене або доставлене).");
                return RedirectToAction("History");
            }

            return View(result.Value);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelConfirm(int id, string reason)
        {
            var result = await _orderService.CancelOrderAsync(id, CurrentUserId!, reason);

            if (result.IsFailure)
            {
                SetErrorMessage(result.ErrorMessage);
            }
            else
            {
                SetSuccessMessage("Ваше замовлення успішно скасовано.");
            }

            return RedirectToAction("History");
        }

        [HttpGet]
        public async Task<IActionResult> Return(int id)
        {
            var result = await _orderService.GetOrderByIdAndBuyerAsync(id, CurrentUserId!);

            if (result.IsFailure)
            {
                SetErrorMessage(result.ErrorMessage);
                return RedirectToAction("History");
            }

            if (result.Value.Status != "Delivered")
            {
                SetErrorMessage("Повернути можна лише отримані (доставлені) замовлення.");
                return RedirectToAction("History");
            }

            return View(result.Value);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReturnConfirm(int id, string reason)
        {
            var result = await _orderService.RequestReturnAsync(id, CurrentUserId!, reason);

            if (result.IsFailure)
            {
                SetErrorMessage(result.ErrorMessage);
            }
            else
            {
                SetSuccessMessage("Ваш запит на повернення надіслано продавцю. Очікуйте підтвердження.");
            }

            return RedirectToAction("History");
        }

        public IActionResult Success() => View();
    }
}