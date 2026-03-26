using MadeByMe.Application.DTOs;
using MadeByMe.Application.Services.Interfaces;
using MadeByMe.Application.ViewModels;
using MadeByMe.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Serilog;

namespace MadeByMe.Web.Controllers
{
    public class PostController : Controller
    {
        private readonly IPostService _postService;
        private readonly ICommentService _commentService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPhotoService _photoService;
        private readonly ICategoryService _categoryService;

        public PostController(
            IPostService postService,
            ICommentService commentService,
            UserManager<ApplicationUser> userManager,
            IPhotoService photoService,
            ICategoryService categoryService)
        {
            _postService = postService;
            _commentService = commentService;
            _userManager = userManager;
            _photoService = photoService;
            _categoryService = categoryService;
        }

        public IActionResult Index(string searchTerm)
        {
            var result = _postService.SearchPosts(searchTerm);

            if (result.IsFailure)
            {
                Log.Warning("Помилка при пошуку постів за запитом '{SearchTerm}': {ErrorMessage}", searchTerm, result.ErrorMessage);
                TempData["Error"] = result.ErrorMessage;
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

            Log.Information("Відображено {Count} постів для запиту '{SearchTerm}'", postsList.Count, searchTerm ?? "усі");
            return View(postsList);
        }

        public IActionResult Details(int id)
        {
            var postResult = _postService.GetPostById(id);
            if (postResult.IsFailure)
            {
                Log.Warning("Спроба перегляду деталей: пост з ID {PostId} не знайдено", id);
                return NotFound(postResult.ErrorMessage);
            }

            var commentsResult = _commentService.GetCommentsForPost(id);
            var comments = commentsResult.IsSuccess ? commentsResult.Value : new List<Comment>();

            var viewModel = new PostDetailsViewModel
            {
                Post = postResult.Value,
                CommentsList = comments,
            };

            Log.Information("Перегляд деталей поста: {PostTitle} (ID: {PostId})", postResult.Value.Title, id);
            return View(viewModel);
        }

        [Authorize(Roles = "Seller")]
        public IActionResult Create()
        {
            LoadCategoriesToViewBag();
            return View();
        }

        [Authorize(Roles = "Seller")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreatePostDto createPostDto)
        {
            if (!ModelState.IsValid)
            {
                Log.Warning("Помилка валідації при створенні нового поста");
                LoadCategoriesToViewBag();
                return View(createPostDto);
            }

            var userId = _userManager.GetUserId(User);
            var postResult = _postService.CreatePost(createPostDto, userId!);

            if (postResult.IsFailure)
            {
                Log.Error("Не вдалося створити пост для користувача {UserId}. Причина: {ErrorMessage}", userId, postResult.ErrorMessage);
                ModelState.AddModelError(string.Empty, postResult.ErrorMessage);
                LoadCategoriesToViewBag();
                return View(createPostDto);
            }

            if (createPostDto.Photo != null)
            {
                // Сервіс зберігає і файл, і запис в базу
                await _photoService.SavePhotoAsync(createPostDto.Photo, postResult.Value.Id);
                Log.Information("Фото для поста {PostId} успішно збережено", postResult.Value.Id);
            }

            Log.Information("Користувач {UserId} успішно створив пост '{PostTitle}' (ID: {PostId})", userId, createPostDto.Title, postResult.Value.Id);
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Seller")]
        public IActionResult Edit(int id)
        {
            var postResult = _postService.GetPostById(id);
            if (postResult.IsFailure)
            {
                Log.Warning("Спроба редагування: пост з ID {PostId} не знайдено", id);
                return NotFound(postResult.ErrorMessage);
            }

            var post = postResult.Value;
            var currentUserId = _userManager.GetUserId(User);

            if (post.SellerId != currentUserId)
            {
                Log.Warning("Відмовлено в доступі: користувач {UserId} намагався редагувати чужий пост {PostId}", currentUserId, id);
                return Forbid();
            }

            var updateDto = new UpdatePostDto
            {
                Title = post.Title,
                Description = post.Description,
                Price = post.Price,
                CategoryId = post.CategoryId,
            };

            LoadCategoriesToViewBag();
            ViewBag.CurrentPhoto = post.Photos?.FirstOrDefault()?.FilePath;

            return View(updateDto);
        }

        [Authorize(Roles = "Seller")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdatePostDto updatePostDto)
        {
            if (!ModelState.IsValid)
            {
                Log.Warning("Помилка валідації при оновленні поста {PostId}", id);
                LoadCategoriesToViewBag();
                return View(updatePostDto);
            }

            var postResult = _postService.GetPostById(id);
            if (postResult.IsFailure)
            {
                return NotFound(postResult.ErrorMessage);
            }

            var post = postResult.Value;
            var currentUserId = _userManager.GetUserId(User);

            if (post.SellerId != currentUserId)
            {
                Log.Warning("Незаконна спроба оновлення: користувач {UserId} намагався змінити пост {PostId}", currentUserId, id);
                return Forbid();
            }

            if (updatePostDto.Photo != null)
            {
                var oldPhoto = post.Photos?.FirstOrDefault();
                if (oldPhoto != null)
                {
                    // Сервіс видаляє і файл, і запис з бази
                    _photoService.DeletePhoto(oldPhoto);
                    Log.Information("Старе фото для поста {PostId} видалено", id);
                }

                await _photoService.SavePhotoAsync(updatePostDto.Photo, post.Id);
                Log.Information("Нове фото для поста {PostId} збережено", id);
            }

            var updateResult = _postService.UpdatePost(id, updatePostDto);
            if (updateResult.IsFailure)
            {
                Log.Error("Не вдалося оновити пост {PostId}. Причина: {ErrorMessage}", id, updateResult.ErrorMessage);
                ModelState.AddModelError(string.Empty, updateResult.ErrorMessage);
                LoadCategoriesToViewBag();
                return View(updatePostDto);
            }

            Log.Information("Користувач {UserId} успішно оновив пост {PostId}", currentUserId, id);
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Seller")]
        public IActionResult Delete(int id)
        {
            var postResult = _postService.GetPostById(id);
            if (postResult.IsFailure)
            {
                Log.Warning("Спроба видалення: пост {PostId} не знайдено", id);
                return NotFound(postResult.ErrorMessage);
            }

            var post = postResult.Value;
            var currentUserId = _userManager.GetUserId(User);

            if (post.SellerId != currentUserId)
            {
                Log.Warning("Спроба видалення: користувач {UserId} не має прав на видалення поста {PostId}", currentUserId, id);
                return Forbid();
            }

            return View(post);
        }

        [Authorize(Roles = "Seller")]
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var postResult = _postService.GetPostById(id);
            if (postResult.IsFailure)
            {
                return NotFound(postResult.ErrorMessage);
            }

            var post = postResult.Value;
            var currentUserId = _userManager.GetUserId(User);

            if (post.SellerId != currentUserId)
            {
                Log.Warning("Незаконна спроба підтвердження видалення: користувач {UserId}, пост {PostId}", currentUserId, id);
                return Forbid();
            }

            if (post.Photos != null)
            {
                foreach (var photo in post.Photos)
                {
                    _photoService.DeletePhoto(photo);
                }

                Log.Information("Усі фото, пов'язані з постом {PostId}, видалено", id);
            }

            var deleteResult = _postService.DeletePost(id);
            if (deleteResult.IsFailure)
            {
                Log.Error("Помилка при видаленні поста {PostId}. Причина: {ErrorMessage}", id, deleteResult.ErrorMessage);
                TempData["Error"] = deleteResult.ErrorMessage;
                return RedirectToAction(nameof(Index));
            }

            Log.Information("Пост {PostId} успішно видалено користувачем {UserId}", id, currentUserId);
            return RedirectToAction(nameof(Index));
        }

        private void LoadCategoriesToViewBag()
        {
            var categoriesResult = _categoryService.GetAllCategories();

            var categories = categoriesResult.IsSuccess
                ? categoriesResult.Value.Select(c => new SelectListItem
                {
                    Value = c.CategoryId.ToString(),
                    Text = c.Name,
                }).ToList()
                : new List<SelectListItem>();

            ViewBag.Categories = categories;
        }
    }
}