using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MadeByMe.Application.Services.Interfaces;
using MadeByMe.Application.DTOs;
using MadeByMe.Infrastructure.Data;
using MadeByMe.Application.ViewModels;



namespace MadeByMe.src.Controllers
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
				return BadRequest();

			var result = _buyerCartService.AddToCart(buyerId, addToCartDto);
			return Ok(result);
		}
	}
}