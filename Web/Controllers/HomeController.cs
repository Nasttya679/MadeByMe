using System.Diagnostics;
using MadeByMe.Application.DTOs;
using MadeByMe.Application.Services.Interfaces;
using MadeByMe.Application.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace MadeByMe.Web.Controllers
{
    public class HomeController : BaseController
    {
        private readonly IPostService _postService;

        public HomeController(IPostService postService)
        {
            _postService = postService;
        }

        public async Task<IActionResult> Index()
        {
            var result = await _postService.GetAllPostsAsync();

            if (result.IsFailure)
            {
                Log.Warning("Не вдалося завантажити список постів для головної сторінки. Причина: {ErrorMessage}", result.ErrorMessage);
                return View(new List<PostResponseDto>());
            }

            var postsList = result.Value.Select(post => new PostResponseDto
            {
                Id = post.Id,
                Title = post.Title,
                Description = post.Description,
                Price = post.Price,
                PhotoUrl = post.Photos?.FirstOrDefault()?.FilePath ?? "/images/default.jpg",
                Rating = post.Rating,
                Status = post.Status,
                CategoryName = post.Category,
                SellerName = post.Seller,
                CreatedAt = post.CreatedAt,
            }).ToList();

            Log.Information("Головна сторінка успішно завантажена. Відображено постів: {PostCount}", postsList.Count);
            return View(postsList);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            Log.Error("Сталася помилка в додатку. Ідентифікатор запиту (RequestId): {RequestId}", requestId);
            return View(new ErrorViewModel
            {
                RequestId = requestId,
            });
        }
    }
}