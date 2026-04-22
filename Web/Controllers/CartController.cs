using Humanizer;
using MadeByMe.Application.DTOs;
using MadeByMe.Application.Services.Interfaces;
using MadeByMe.Application.ViewModels;
using MadeByMe.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace MadeByMe.Web.Controllers
{
    [Authorize]
    public class CartController : BaseController
    {
        private readonly ICartService _cartService;
        private readonly IBuyerCartService _buyerCartService;

        public CartController(ICartService cartService, IBuyerCartService buyerCartService)
        {
            _cartService = cartService;
            _buyerCartService = buyerCartService;
        }

        public async Task<IActionResult> Index()
        {
            var buyerId = CurrentUserId;

            var result = await _cartService.GetUserCartAsync(buyerId!);

            if (result.IsFailure)
            {
                Log.Warning("Не вдалося завантажити кошик для покупця {BuyerId}. Причина: {ErrorMessage}", buyerId, result.ErrorMessage);
                SetErrorMessage(result.ErrorMessage);
                return View(new CartViewModel());
            }

            return View(result.Value);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(AddToCartDto dto)
        {
            var buyerId = CurrentUserId;

            var result = await _buyerCartService.AddToCartAsync(buyerId!, dto);

            if (result.IsFailure)
            {
                Log.Warning("Не вдалося додати товар {PostId} у кошик для покупця {BuyerId}. Причина: {ErrorMessage}", dto.PostId, buyerId, result.ErrorMessage);
                return BadRequest(result.ErrorMessage);
            }

            Log.Information("Товар {PostId} успішно додано у кошик покупцем {BuyerId}", dto.PostId, buyerId);
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromCart(int postId)
        {
            var buyerId = CurrentUserId;

            var result = await _buyerCartService.RemoveFromCartAsync(buyerId!, postId);

            if (result.IsFailure)
            {
                Log.Warning("Не вдалося видалити товар {PostId} з кошика покупця {BuyerId}. Причина: {ErrorMessage}", postId, buyerId, result.ErrorMessage);
                SetErrorMessage(result.ErrorMessage);
                return RedirectToAction("Index");
            }

            Log.Information("Товар {PostId} видалено з кошика покупцем {BuyerId}", postId, buyerId);
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateQuantity(int productId, string action)
        {
            var buyerId = CurrentUserId;

            var cartResult = await _cartService.GetUserCartEntityAsync(buyerId!);
            if (cartResult.IsFailure)
            {
                return NotFound(cartResult.ErrorMessage);
            }

            var cart = cartResult.Value;

            var cartItem = cart.BuyerCarts?.FirstOrDefault(bc => bc.PostId == productId);
            if (cartItem == null)
            {
                return NotFound("Товар у кошику не знайдено.");
            }

            if (action == "increase")
            {
                cartItem.Quantity++;
            }
            else if (action == "decrease" && cartItem.Quantity > 1)
            {
                cartItem.Quantity--;
            }

            var updateResult = await _cartService.UpdateCartItemAsync(cartItem);
            if (updateResult.IsFailure)
            {
                Log.Warning("Не вдалося оновити кількість товару {PostId} у кошику покупця {BuyerId}. Причина: {ErrorMessage}", productId, buyerId, updateResult.ErrorMessage);
                SetErrorMessage(updateResult.ErrorMessage);
            }
            else
            {
                Log.Information("Покупець {BuyerId} змінив кількість товару {PostId} на {Quantity}", buyerId, productId, cartItem.Quantity);
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> GetTotalPrice()
        {
            var buyerId = CurrentUserId;

            var cartResult = await _cartService.GetUserCartEntityAsync(buyerId!);
            if (cartResult.IsFailure)
            {
                return NotFound(cartResult.ErrorMessage);
            }

            var totalResult = await _cartService.GetCartTotalAsync(cartResult.Value.CartId);
            if (totalResult.IsFailure)
            {
                return BadRequest(totalResult.ErrorMessage);
            }

            return View("CartTotal", totalResult.Value);
        }

        [HttpPost]
        public async Task<IActionResult> Checkout()
        {
            var buyerId = CurrentUserId;

            var cartResult = await _cartService.GetUserCartEntityAsync(buyerId!);

            if (cartResult.IsFailure || cartResult.Value.BuyerCarts == null || !cartResult.Value.BuyerCarts.Any())
            {
                Log.Warning("Покупець {BuyerId} намагався оформити замовлення з порожнім кошиком", buyerId);
                return View("EmptyCartError");
            }

            var clearResult = await _cartService.ClearCartAsync(cartResult.Value.CartId);
            if (clearResult.IsFailure)
            {
                Log.Error("КРИТИЧНА ПОМИЛКА: Покупець {BuyerId} оформив замовлення, але не вдалося очистити кошик {CartId}. Причина: {ErrorMessage}", buyerId, cartResult.Value.CartId, clearResult.ErrorMessage);
                SetErrorMessage(clearResult.ErrorMessage);
                return RedirectToAction("Index");
            }

            Log.Information("УСПІШНЕ ЗАМОВЛЕННЯ: Покупець {BuyerId} успішно оформив замовлення і кошик очищено", buyerId);
            return View("Success");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ClearCart()
        {
            var buyerId = CurrentUserId;
            var cartResult = await _cartService.GetUserCartEntityAsync(buyerId!);

            if (cartResult.IsSuccess)
            {
                var clearResult = await _cartService.ClearCartAsync(cartResult.Value.CartId);
                if (clearResult.IsFailure)
                {
                    Log.Warning("Не вдалося очистити кошик для покупця {BuyerId}. Причина: {ErrorMessage}", buyerId, clearResult.ErrorMessage);
                    return BadRequest(clearResult.ErrorMessage);
                }

                Log.Information("Покупець {BuyerId} повністю очистив свій кошик", buyerId);
            }

            return Ok();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCartSilent(AddToCartDto dto)
        {
            var buyerId = CurrentUserId;

            var result = await _buyerCartService.AddToCartAsync(buyerId!, dto);

            if (result.IsFailure)
            {
                Log.Warning("Не вдалося додати товар {PostId} у кошик. Причина: {ErrorMessage}", dto.PostId, result.ErrorMessage);
                return Json(new { success = false, message = result.ErrorMessage });
            }

            Log.Information("Товар {PostId} успішно додано у кошик", dto.PostId);

            var cartResult = await _cartService.GetUserCartEntityAsync(buyerId!);
            int newCount = 0;
            if (cartResult.IsSuccess && cartResult.Value.BuyerCarts != null)
            {
                newCount = cartResult.Value.BuyerCarts.Sum(c => c.Quantity);
            }

            return Json(new { success = true, cartCount = newCount });
        }
    }
}