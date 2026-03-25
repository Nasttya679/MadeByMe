using System.Linq;
using MadeByMe.Application.DTOs;
using MadeByMe.Application.Services.Interfaces;
using MadeByMe.Application.ViewModels;
using MadeByMe.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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
            return BadRequest(result.ErrorMessage);
        }

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
            TempData["Error"] = result.ErrorMessage;
            return RedirectToAction("Index");
        }

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
            TempData["Error"] = updateResult.ErrorMessage;
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
            return View("EmptyCartError");
        }

        var clearResult = _cartService.ClearCart(cartResult.Value.CartId);
        if (clearResult.IsFailure)
        {
            TempData["Error"] = clearResult.ErrorMessage;
            return RedirectToAction("Index");
        }

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
                return BadRequest(clearResult.ErrorMessage);
            }
        }

        return Ok();
    }
}