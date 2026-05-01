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
        private readonly IBuyerCartService _buyerCartService;
        private readonly IWishlistService _wishlistService;

        public PostController(
            IPostService postService,
            ICommentService commentService,
            IPhotoService photoService,
            ICategoryService categoryService,
            IBuyerCartService buyerCartService,
            IWishlistService wishlistService)
        {
            _postService = postService;
            _commentService = commentService;
            _photoService = photoService;
            _categoryService = categoryService;
            _buyerCartService = buyerCartService;
            _wishlistService = wishlistService;
        }

        public async Task<IActionResult> Index(string? searchTerm, int? categoryId, string? sortBy, string searchType = "products")
        {
            var result = await _postService.GetFilteredPostsAsync(searchTerm, categoryId, sortBy, 1, searchType);

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
            ViewBag.CurrentSearchType = searchType;

            var currentUserId = CurrentUserId;
            var favoritePostIds = new HashSet<int>();

            if (currentUserId != null)
            {
                var wishlistResult = await _wishlistService.GetUserWishlistAsync(currentUserId);
                if (wishlistResult.IsSuccess)
                {
                    favoritePostIds = wishlistResult.Value.Select(w => w.PostId).ToHashSet();
                }
            }

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
                SellerId = post.SellerId,
                CreatedAt = post.CreatedAt,
                IsFavorite = favoritePostIds.Contains(post.Id),
                IsDeleted = post.IsDeleted,
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

            bool isFavorite = false;
            var currentUserId = CurrentUserId;

            if (currentUserId != null)
            {
                var wishlistResult = await _wishlistService.GetUserWishlistAsync(currentUserId);
                if (wishlistResult.IsSuccess)
                {
                    isFavorite = wishlistResult.Value.Any(w => w.PostId == id);
                }
            }

            var viewModel = new PostDetailsViewModel
            {
                Post = postResult.Value,
                CommentsList = comments,
                IsFavorite = isFavorite,
            };

            Log.Information("Перегляд деталей поста: {PostTitle} (ID: {PostId})", postResult.Value.Title, id);
            return View(viewModel);
        }

        [Authorize(Roles = "Seller, Admin")]
        public async Task<IActionResult> Create()
        {
            await LoadCategoriesToViewBagAsync();
            return View();
        }

        [Authorize(Roles = "Seller, Admin")]
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

            if (User.IsInRole("Admin"))
            {
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction("Storefront", "Seller", new { id = userId });
        }

        [Authorize(Roles = "Seller, Admin")]
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

            if (post.SellerId != currentUserId && !User.IsInRole("Admin"))
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

        [Authorize(Roles = "Seller, Admin")]
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

            if (post.SellerId != currentUserId && !User.IsInRole("Admin"))
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

            if (User.IsInRole("Admin"))
            {
                return RedirectToAction(nameof(Details), new { id = id });
            }

            return RedirectToAction("Storefront", "Seller", new { id = currentUserId });
        }

        [Authorize(Roles = "Seller, Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _postService.GetPostByIdAsync(id);

            if (result.IsFailure)
            {
                return NotFound(result.ErrorMessage);
            }

            var post = result.Value;
            var currentUserId = CurrentUserId;

            var isAdmin = User.IsInRole("Admin");
            var isOwner = post.SellerId == currentUserId;

            if (!isAdmin && !isOwner)
            {
                return Forbid();
            }

            return View(post);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _postService.GetPostByIdAsync(id);

            if (result.IsFailure)
            {
                return NotFound(result.ErrorMessage);
            }

            var post = result.Value;

            if (!User.IsInRole("Admin") && post.SellerId != CurrentUserId)
            {
                return Forbid();
            }

            var deleteResult = await _postService.DeletePostAsync(id, CurrentUserId!);

            if (deleteResult.IsFailure)
            {
                SetErrorMessage(deleteResult.ErrorMessage);
                return RedirectToAction(nameof(Index));
            }

            SetSuccessMessage("Виріб успішно переміщено до кошика!");

            if (User.IsInRole("Admin"))
            {
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction("Storefront", "Seller", new { id = CurrentUserId });
        }

        [Authorize(Roles = "Seller, Admin")]
        public async Task<IActionResult> DeletedPosts()
        {
            var currentUserId = CurrentUserId;
            var isAdmin = User.IsInRole("Admin");

            string? userIdFilter = isAdmin ? null : currentUserId;

            var result = await _postService.GetDeletedPostsAsync(userIdFilter);

            if (result.IsFailure)
            {
                Log.Warning("Помилка завантаження видалених постів: {ErrorMessage}", result.ErrorMessage);
                SetErrorMessage("Не вдалося завантажити кошик видалених товарів.");
                return RedirectToAction(nameof(Index));
            }

            var deletedPostsList = result.Value.Select(post => new PostResponseDto
            {
                Id = post.Id,
                Title = post.Title,
                Price = post.Price,
                PhotoUrl = post.Photos.FirstOrDefault()?.FilePath ?? "/images/default.jpg",
                CategoryName = post.Category,
                SellerName = post.Seller,
                CreatedAt = post.CreatedAt,
                IsDeleted = post.IsDeleted,
            }).ToList();

            Log.Information("Користувач {UserId} переглядає кошик видалених товарів. Знайдено: {Count}", currentUserId, deletedPostsList.Count);

            return View(deletedPostsList);
        }

        [Authorize(Roles = "Seller, Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(int id)
        {
            var result = await _postService.GetPostByIdAsync(id);

            if (result.IsFailure)
            {
                return NotFound(result.ErrorMessage);
            }

            var post = result.Value;

            if (!User.IsInRole("Admin") && post.SellerId != CurrentUserId)
            {
                Log.Warning("Користувач {UserId} намагався відновити чужий товар {PostId}", CurrentUserId, id);
                return Forbid();
            }

            var restoreResult = await _postService.RestorePostAsync(id);

            if (restoreResult.IsFailure)
            {
                SetErrorMessage(restoreResult.ErrorMessage);
                return RedirectToAction(nameof(DeletedPosts));
            }

            SetSuccessMessage($"Товар '{post.Title}' успішно відновлено і повернуто на вітрину!");

            if (User.IsInRole("Admin"))
            {
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction("Storefront", "Seller", new { id = CurrentUserId });
        }

        [Authorize(Roles = "Seller, Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HardDelete(int id)
        {
            var result = await _postService.GetPostByIdAsync(id);

            if (result.IsFailure)
            {
                return NotFound(result.ErrorMessage);
            }

            var post = result.Value;

            if (!User.IsInRole("Admin") && post.SellerId != CurrentUserId)
            {
                Log.Warning("Користувач {UserId} намагався назавжди видалити чужий товар {PostId}", CurrentUserId, id);
                return Forbid();
            }

            foreach (var photo in post.Photos.ToList())
            {
                try
                {
                    await _photoService.DeletePhotoAsync(photo);
                }
                catch (Exception ex)
                {
                    Log.Warning("Помилка видалення фото при повному видаленні товару: {Message}", ex.Message);
                }
            }

            var deleteResult = await _postService.HardDeletePostAsync(id);

            if (deleteResult.IsFailure)
            {
                SetErrorMessage(deleteResult.ErrorMessage);
            }
            else
            {
                SetSuccessMessage($"Товар '{post.Title}' було видалено назавжди!");
            }

            return RedirectToAction(nameof(DeletedPosts));
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