using MadeByMe.Application.DTOs;
using MadeByMe.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MadeByMe.Web.Controllers
{
    public class BuyerCartController : Controller
    {
        private readonly IBuyerCartService _buyerCartService;

        public BuyerCartController(IBuyerCartService buyerCartService)
        {
            _buyerCartService = buyerCartService;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddToCart(AddToCartDto addToCartDto, string buyerId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Недійсні дані форми.");
            }

            var result = _buyerCartService.AddToCart(buyerId, addToCartDto);

            if (result.IsFailure)
            {
                return BadRequest(result.ErrorMessage);
            }

            return Ok("Товар успішно додано до кошика!");
        }
    }
}