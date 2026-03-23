using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using MadeByMe.Application.Services.Interfaces;
using MadeByMe.Application.Services;
using MadeByMe.Application.DTOs;
using MadeByMe.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using MadeByMe.Infrastructure.Data;
using MadeByMe.Application.ViewModels;


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
        var cartViewModel = _cartService.GetUserCart(buyerId);

        return View(cartViewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AddToCart(AddToCartDto dto)
    {
        var buyerId = _userManager.GetUserId(User);

        var result = _buyerCartService.AddToCart(buyerId, dto);
        if (!result)
        {
            return BadRequest("Помилка при додаванні товару");
        }

        return RedirectToAction("Index", new { buyerId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult RemoveFromCart(int postId)
    {
        var buyerId = _userManager.GetUserId(User);

        var result = _buyerCartService.RemoveFromCart(buyerId, postId);
        if (!result)
        {
            return NotFound();
        }

        return RedirectToAction("Index", new { buyerId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult UpdateQuantity(int productId, string action)
    {
        var buyerId = _userManager.GetUserId(User);
        var cart = _cartService.GetUserCartEntity(buyerId);
        
        if (cart == null)
        {
            return NotFound();
        }

        var cartItem = cart.BuyerCarts.FirstOrDefault(bc => bc.PostId == productId);
        if (cartItem == null)
        {
            return NotFound();
        }

        if (action == "increase")
        {
            cartItem.Quantity++;
        }
        else if (action == "decrease" && cartItem.Quantity > 1)
        {
            cartItem.Quantity--;
        }

        _cartService.UpdateCartItem(cartItem);
        return RedirectToAction("Index");
    }

    public IActionResult GetTotalPrice()
    {
        var buyerId = _userManager.GetUserId(User);

        var cart = _cartService.GetUserCartEntity(buyerId);
        if (cart == null)
        {
            return NotFound();
        }

        var total = _cartService.GetCartTotal(cart.CartId);
        return View("CartTotal", total);
    }

    [HttpPost]
    public IActionResult Checkout()
    {
        var buyerId = _userManager.GetUserId(User);

        var cart = _cartService.GetUserCartEntity(buyerId);
        if (cart == null || !cart.BuyerCarts.Any())
        {
            return View("EmptyCartError");
        }

        _cartService.ClearCart(cart.CartId);
        return View("CheckoutSuccess");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ClearCart()
    {
        var buyerId = _userManager.GetUserId(User);
        var cart = _cartService.GetUserCartEntity(buyerId);
        
        if (cart != null)
        {
            _cartService.ClearCart(cart.CartId);
        }
        
        return Ok();
    }
}