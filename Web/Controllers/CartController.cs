using Humanizer;
using MadeByMe.Application.DTOs;
using MadeByMe.Application.Services.Interfaces;
using MadeByMe.Application.ViewModels;
using MadeByMe.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Serilog;

public class CartController : Controller
{
    private readonly ICartService _cartService;
    private readonly IBuyerCartService _buyerCartService;
    private readonly UserManager<ApplicationUser> _userManager;

    public CartController(ICartService cartService, IBuyerCartService buyerCartService, UserManager<ApplicationUser> userManager)
    {
        _cartService = cartService;
        _buyerCartService = buyerCartService;
        _userManager = userManager;
    }

    public IActionResult Index()
    {
        var buyerId = _userManager.GetUserId(User);

        var result = _cartService.GetUserCart(buyerId!);

        if (result.IsFailure)
        {
            Log.Warning("Не вдалося завантажити кошик для покупця {BuyerId}. Причина: {ErrorMessage}", buyerId, result.ErrorMessage);
            TempData["Error"] = result.ErrorMessage;
            return View(new CartViewModel());
        }

        return View(result.Value);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AddToCart(AddToCartDto dto)
    {
        var buyerId = _userManager.GetUserId(User);

        var result = _buyerCartService.AddToCart(buyerId!, dto);

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
    public IActionResult RemoveFromCart(int postId)
    {
        var buyerId = _userManager.GetUserId(User);

        var result = _buyerCartService.RemoveFromCart(buyerId!, postId);

        if (result.IsFailure)
        {
            Log.Warning("Не вдалося видалити товар {PostId} з кошика покупця {BuyerId}. Причина: {ErrorMessage}", postId, buyerId, result.ErrorMessage);
            TempData["Error"] = result.ErrorMessage;
            return RedirectToAction("Index");
        }

        Log.Information("Товар {PostId} видалено з кошика покупцем {BuyerId}", postId, buyerId);
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult UpdateQuantity(int productId, string action)
    {
        var buyerId = _userManager.GetUserId(User);

        var cartResult = _cartService.GetUserCartEntity(buyerId!);
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

        var updateResult = _cartService.UpdateCartItem(cartItem);
        if (updateResult.IsFailure)
        {
            Log.Warning("Не вдалося оновити кількість товару {PostId} у кошику покупця {BuyerId}. Причина: {ErrorMessage}", productId, buyerId, updateResult.ErrorMessage);
            TempData["Error"] = updateResult.ErrorMessage;
        }
        else
        {
            Log.Information("Покупець {BuyerId} змінив кількість товару {PostId} на {Quantity}", buyerId, productId, cartItem.Quantity);
        }

        return RedirectToAction("Index");
    }

    public IActionResult GetTotalPrice()
    {
        var buyerId = _userManager.GetUserId(User);

        var cartResult = _cartService.GetUserCartEntity(buyerId!);
        if (cartResult.IsFailure)
        {
            return NotFound(cartResult.ErrorMessage);
        }

        var totalResult = _cartService.GetCartTotal(cartResult.Value.CartId);
        if (totalResult.IsFailure)
        {
            return BadRequest(totalResult.ErrorMessage);
        }

        return View("CartTotal", totalResult.Value);
    }

    [HttpPost]
    public IActionResult Checkout()
    {
        var buyerId = _userManager.GetUserId(User);

        var cartResult = _cartService.GetUserCartEntity(buyerId!);

        if (cartResult.IsFailure || cartResult.Value.BuyerCarts == null || !cartResult.Value.BuyerCarts.Any())
        {
            Log.Warning("Покупець {BuyerId} намагався оформити замовлення з порожнім кошиком", buyerId);
            return View("EmptyCartError");
        }

        var clearResult = _cartService.ClearCart(cartResult.Value.CartId);
        if (clearResult.IsFailure)
        {
            Log.Error("КРИТИЧНА ПОМИЛКА: Покупець {BuyerId} оформив замовлення, але не вдалося очистити кошик {CartId}. Причина: {ErrorMessage}", buyerId, cartResult.Value.CartId, clearResult.ErrorMessage);
            TempData["Error"] = clearResult.ErrorMessage;
            return RedirectToAction("Index");
        }

        Log.Information("УСПІШНЕ ЗАМОВЛЕННЯ: Покупець {BuyerId} успішно оформив замовлення і кошик очищено", buyerId);
        return View("CheckoutSuccess");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ClearCart()
    {
        var buyerId = _userManager.GetUserId(User);
        var cartResult = _cartService.GetUserCartEntity(buyerId!);

        if (cartResult.IsSuccess)
        {
            var clearResult = _cartService.ClearCart(cartResult.Value.CartId);
            if (clearResult.IsFailure)
            {
                Log.Warning("Не вдалося очистити кошик для покупця {BuyerId}. Причина: {ErrorMessage}", buyerId, clearResult.ErrorMessage);
                return BadRequest(clearResult.ErrorMessage);
            }

            Log.Information("Покупець {BuyerId} повністю очистив свій кошик", buyerId);
        }

        return Ok();
    }
}