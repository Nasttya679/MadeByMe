using Microsoft.AspNetCore.Mvc;
using MadeByMe.Services;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using MadeByMe.Models;

namespace MadeByMe.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly PostService _postService;

        public HomeController(ILogger<HomeController> logger, PostService postService)
        {
            _logger = logger;
            _postService = postService;
        }

        public IActionResult Index()
        {
            var posts = _postService.GetAllPosts();

            var postsList = posts.Select(post => new DTOs.PostResponseDto
            {
                Id = post.Id,
                Title = post.Title,
                Description = post.Description,
                Price = post.Price,
                PhotoUrl = post.Photos?.FirstOrDefault()?.FilePath ?? "/images/default.jpg",
                Rating = 0.0m,
                Status = "Активний",
                CreatedAt = DateTime.UtcNow,
                CategoryName = post.Category,
                SellerName = post.Seller
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
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}