using MadeByMe.Application.DTOs;
using MadeByMe.Application.Services.Interfaces;
using MadeByMe.Application.ViewModels;
using MadeByMe.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Serilog;

namespace MadeByMe.Web.Controllers
{
    public class PostController : BaseController
    {
        private readonly IPostService _postService;
        private readonly ICommentService _commentService;
        private readonly IPhotoService _photoService;
        private readonly ICategoryService _categoryService;
        private readonly IBuyerCartService _buyerCartService; // Додано для очищення кошиків

        public PostController(
            IPostService postService,
            ICommentService commentService,
            IPhotoService photoService,
            ICategoryService categoryService,
            IBuyerCartService buyerCartService)
        {
            _postService = postService;
            _commentService = commentService;
            _photoService = photoService;
            _categoryService = categoryService;
            _buyerCartService = buyerCartService;
        }

        public async Task<IActionResult> Index(string? searchTerm, int? categoryId, string? sortBy)
        {
            var result = await _postService.GetFilteredPostsAsync(searchTerm, categoryId, sortBy);

            if (result.IsFailure)
            {
                Log.Warning("Помилка при пошуку постів: {ErrorMessage}", result.ErrorMessage);
                SetErrorMessage(result.ErrorMessage);
                return View(new List<PostResponseDto>());
            }

            await LoadCategoriesToViewBagAsync();

            ViewBag.CurrentSearch = searchTerm;
            ViewBag.CurrentCategory = categoryId;
            ViewBag.CurrentSort = sortBy;

            var postsList = result.Value.Select(post => new PostResponseDto
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
                CreatedAt = post.CreatedAt,
            }).ToList();

            Log.Information("Каталог відображено успішно. Знайдено товарів: {Count}", postsList.Count);
            return View(postsList);
        }

        public async Task<IActionResult> Details(int id)
        {
            var postResult = await _postService.GetPostByIdAsync(id);
            if (postResult.IsFailure)
            {
                Log.Warning("Спроба перегляду деталей: пост з ID {PostId} не знайдено", id);
                return NotFound(postResult.ErrorMessage);
            }

            var commentsResult = await _commentService.GetCommentsForPostAsync(id);
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
        public async Task<IActionResult> Create()
        {
            await LoadCategoriesToViewBagAsync();
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
                await LoadCategoriesToViewBagAsync();
                return View(createPostDto);
            }

            var userId = CurrentUserId;
            var postResult = await _postService.CreatePostAsync(createPostDto, userId!);

            if (postResult.IsFailure)
            {
                Log.Error("Не вдалося створити пост для користувача {UserId}. Причина: {ErrorMessage}", userId, postResult.ErrorMessage);
                AddErrorToModelState(postResult.ErrorMessage);
                await LoadCategoriesToViewBagAsync();
                return View(createPostDto);
            }

            if (createPostDto.Photo != null)
            {
                await _photoService.SavePhotoAsync(createPostDto.Photo, postResult.Value.Id);
                Log.Information("Фото для поста {PostId} успішно збережено", postResult.Value.Id);
            }

            Log.Information("Користувач {UserId} успішно створив пост '{PostTitle}' (ID: {PostId})", userId, createPostDto.Title, postResult.Value.Id);
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> Edit(int id)
        {
            var postResult = await _postService.GetPostByIdAsync(id);
            if (postResult.IsFailure)
            {
                Log.Warning("Спроба редагування: пост з ID {PostId} не знайдено", id);
                return NotFound(postResult.ErrorMessage);
            }

            var post = postResult.Value;
            var currentUserId = CurrentUserId;

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

            await LoadCategoriesToViewBagAsync();

            ViewBag.CurrentPhoto = post.Photos.FirstOrDefault()?.FilePath;

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
                await LoadCategoriesToViewBagAsync();
                return View(updatePostDto);
            }

            var postResult = await _postService.GetPostByIdAsync(id);
            if (postResult.IsFailure)
            {
                return NotFound(postResult.ErrorMessage);
            }

            var post = postResult.Value;
            var currentUserId = CurrentUserId;

            if (post.SellerId != currentUserId)
            {
                Log.Warning("Незаконна спроба оновлення: користувач {UserId} намагався змінити пост {PostId}", currentUserId, id);
                return Forbid();
            }

            if (updatePostDto.Photo != null)
            {
                var oldPhoto = post.Photos.FirstOrDefault();
                if (oldPhoto != null)
                {
                    await _photoService.DeletePhotoAsync(oldPhoto);
                    Log.Information("Старе фото для поста {PostId} видалено", id);
                }

                await _photoService.SavePhotoAsync(updatePostDto.Photo, post.Id);
                Log.Information("Нове фото для поста {PostId} збережено", id);
            }

            var updateResult = await _postService.UpdatePostAsync(id, updatePostDto);
            if (updateResult.IsFailure)
            {
                Log.Error("Не вдалося оновити пост {PostId}. Причина: {ErrorMessage}", id, updateResult.ErrorMessage);
                AddErrorToModelState(updateResult.ErrorMessage);
                await LoadCategoriesToViewBagAsync();
                return View(updatePostDto);
            }

            Log.Information("Користувач {UserId} успішно оновив пост {PostId}", currentUserId, id);
            return RedirectToAction(nameof(Details), new { id = id });
        }

        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> Delete(int id)
        {
            var postResult = await _postService.GetPostByIdAsync(id);
            if (postResult.IsFailure)
            {
                Log.Warning("Спроба видалення: пост {PostId} не знайдено", id);
                return NotFound(postResult.ErrorMessage);
            }

            var post = postResult.Value;
            var currentUserId = CurrentUserId;

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
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var postResult = await _postService.GetPostByIdAsync(id);
            if (postResult.IsFailure)
            {
                return NotFound(postResult.ErrorMessage);
            }

            var post = postResult.Value;
            var currentUserId = CurrentUserId;

            if (post.SellerId != currentUserId)
            {
                Log.Warning("Незаконна спроба підтвердження видалення: користувач {UserId}, пост {PostId}", currentUserId, id);
                return Forbid();
            }

            if (post.Photos != null && post.Photos.Any())
            {
                var photosToDelete = post.Photos.ToList();
                foreach (var photo in photosToDelete)
                {
                    await _photoService.DeletePhotoAsync(photo);
                }

                Log.Information("Усі фото для поста {PostId} видалено", id);
            }

            var commentsResult = await _commentService.GetCommentsForPostAsync(id);
            if (commentsResult.IsSuccess)
            {
                var commentsToDelete = commentsResult.Value.ToList();
                foreach (var comment in commentsToDelete)
                {
                    await _commentService.DeleteCommentAsync(comment.CommentId);
                }

                Log.Information("Усі коментарі для поста {PostId} видалено", id);
            }

            var deleteResult = await _postService.DeletePostAsync(id);
            if (deleteResult.IsFailure)
            {
                Log.Error("Помилка при видаленні поста {PostId}: {ErrorMessage}", id, deleteResult.ErrorMessage);
                SetErrorMessage(deleteResult.ErrorMessage);
                return RedirectToAction(nameof(Index));
            }

            Log.Information("Пост {PostId} успішно видалено користувачем {UserId}", id, currentUserId);
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadCategoriesToViewBagAsync()
        {
            var categoriesResult = await _categoryService.GetAllCategoriesAsync();

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