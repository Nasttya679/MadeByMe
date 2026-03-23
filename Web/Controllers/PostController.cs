using MadeByMe.Application.Services;
using MadeByMe.Application.DTOs;
using MadeByMe.Application.ViewModels;
using MadeByMe.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;
using MadeByMe.Infrastructure.Data;
using MadeByMe.Application.Services.Interfaces;

namespace MadeByMe.src.Controllers
{
    public class PostController : Controller
    {
        private readonly IPostService _postService;
        private readonly ICommentService _commentService;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPhotoService _photoService;

        public PostController(
            IPostService postService, 
            ICommentService commentService, 
            ApplicationDbContext context, 
            UserManager<ApplicationUser> userManager,
            IPhotoService photoService)
        {
            _postService = postService;
            _commentService = commentService;
            _context = context;
            _userManager = userManager;
            _photoService = photoService;
        }

        public IActionResult Index(string searchTerm)
        {
            var posts = _postService.SearchPosts(searchTerm);

            var postsList = posts.Select(post => new PostResponseDto
            {
                Id = post.Id,
                Title = post.Title,
                Description = post.Description,
                Price = post.Price,
                PhotoUrl = post.Photos.FirstOrDefault()?.FilePath ?? "/images/default.jpg",
                Rating = post.Rating,
                Status = post.Status,
                CategoryName = post.Category,
                SellerName = post.Seller,
                CreatedAt = post.CreatedAt
            }).ToList();

            return View(postsList);
        }

        public IActionResult Details(int id) 
        {
            var post = _postService.GetPostById(id);
            if (post == null)
                return NotFound();

            var comments = _commentService.GetCommentsForPost(id);

            var viewModel = new PostDetailsViewModel
            {
                Post = post,
                CommentsList = comments
            };

            return View(viewModel);
        }

        [Authorize(Roles = "Seller")]
        public IActionResult Create()
        {
            var categories = _context.Categories.Select(c => new SelectListItem
            {
                Value = c.CategoryId.ToString(),
                Text = c.Name
            }).ToList();

            ViewBag.Categories = categories;
            return View();
        }

        [Authorize(Roles = "Seller")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreatePostDto createPostDto)
        {
            if (!ModelState.IsValid)
            {
                var categories = _context.Categories.Select(c => new SelectListItem
                {
                    Value = c.CategoryId.ToString(),
                    Text = c.Name
                }).ToList();

                ViewBag.Categories = categories;
                return View(createPostDto);
            }

            var userId = _userManager.GetUserId(User);
            var post = _postService.CreatePost(createPostDto, userId);

            if (createPostDto.Photo != null)
            {
                var photo = await _photoService.SavePhotoAsync(createPostDto.Photo, post.Id);
                _context.Photos.Add(photo);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Seller")]
        public IActionResult Edit(int id)
        {
            var post = _postService.GetPostById(id);
            if (post == null)
                return NotFound();

            var currentUserId = _userManager.GetUserId(User); 
            if (post.SellerId != currentUserId)
                return Forbid();

            var updateDto = new UpdatePostDto
            {
                Title = post.Title,
                Description = post.Description,
                Price = post.Price,
                CategoryId = post.CategoryId
            };

            var categories = _context.Categories.Select(c => new SelectListItem
            {
                Value = c.CategoryId.ToString(),
                Text = c.Name
            }).ToList();

            ViewBag.Categories = categories;
            ViewBag.CurrentPhoto = post.Photos.FirstOrDefault()?.FilePath;

            return View(updateDto);
        }

        [Authorize(Roles = "Seller")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdatePostDto updatePostDto)
        {
            if (!ModelState.IsValid)
                return View(updatePostDto);

            var post = _postService.GetPostById(id);
            if (post == null)
                return NotFound();

            var currentUserId = _userManager.GetUserId(User);
            if (post.SellerId != currentUserId)
                return Forbid();

            if (updatePostDto.Photo != null)
            {
                // Delete old photo
                var oldPhoto = post.Photos.FirstOrDefault();
                if (oldPhoto != null)
                {
                    _photoService.DeletePhoto(oldPhoto);
                    _context.Photos.Remove(oldPhoto);
                }

                // Save new photo
                var newPhoto = await _photoService.SavePhotoAsync(updatePostDto.Photo, post.Id);
                _context.Photos.Add(newPhoto);
            }

            var updatedPost = _postService.UpdatePost(id, updatePostDto);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Seller")]
        public IActionResult Delete(int id)
        {
            var post = _postService.GetPostById(id);
            if (post == null)
                return NotFound();

            var currentUserId = _userManager.GetUserId(User);
            if (post.SellerId != currentUserId)
                return Forbid();

            return View(post);
        }

        [Authorize(Roles = "Seller")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var post = _postService.GetPostById(id);
            if (post == null)
                return NotFound();

            var currentUserId = _userManager.GetUserId(User);
            if (post.SellerId != currentUserId)
                return Forbid();

            // Delete associated photos
            foreach (var photo in post.Photos)
            {
                _photoService.DeletePhoto(photo);
                _context.Photos.Remove(photo);
            }

            bool isDeleted = _postService.DeletePost(id);
            await _context.SaveChangesAsync();

            if (!isDeleted)
                return NotFound();

            return RedirectToAction(nameof(Index));
        }
    }
}