using System.Diagnostics;
using MadeByMe.Application.DTOs;
using MadeByMe.Application.Services.Interfaces;
using MadeByMe.Application.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace MadeByMe.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IPostService _postService;

        public HomeController(IPostService postService)
        {
            _postService = postService;
        }

        public IActionResult Index()
        {
            var result = _postService.GetAllPosts();

            if (result.IsFailure)
            {
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

            return View(postsList);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
            });
        }
    }
}