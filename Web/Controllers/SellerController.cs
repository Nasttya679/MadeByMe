using MadeByMe.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace MadeByMe.Web.Controllers
{
    [Authorize(Roles = "Seller")]
    public class SellerController : BaseController
    {
        private readonly IOrderService _orderService;

        public SellerController(IOrderService orderService)
        {
            _orderService = orderService;
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
    }
}