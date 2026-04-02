using MadeByMe.Application.DTOs;
using MadeByMe.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace MadeByMe.Web.Controllers
{
    [Authorize]
    public class BuyerCartController : BaseController
    {
        private readonly IBuyerCartService _buyerCartService;

        public BuyerCartController(IBuyerCartService buyerCartService)
        {
            _buyerCartService = buyerCartService;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(AddToCartDto addToCartDto, string buyerId)
        {
            if (!ModelState.IsValid)
            {
                Log.Warning("Помилка валідації при спробі додати товар у кошик для покупця {BuyerId}", buyerId);
                return BadRequest("Недійсні дані форми.");
            }

            var result = await _buyerCartService.AddToCartAsync(buyerId, addToCartDto);

            if (result.IsFailure)
            {
                Log.Warning("Не вдалося додати товар {PostId} у кошик для покупця {BuyerId}. Причина: {ErrorMessage}", addToCartDto.PostId, buyerId, result.ErrorMessage);
                return BadRequest(result.ErrorMessage);
            }

            Log.Information("Товар {PostId} успішно додано у кошик для покупця {BuyerId}", addToCartDto.PostId, buyerId);
            return Ok("Товар успішно додано до кошика!");
        }
    }
}