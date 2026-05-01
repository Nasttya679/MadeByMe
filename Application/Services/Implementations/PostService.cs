using MadeByMe.Application.Common;
using MadeByMe.Application.DTOs;
using MadeByMe.Application.Services.Interfaces;
using MadeByMe.Domain.Entities;
using MadeByMe.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Options;
using Serilog;

namespace MadeByMe.Application.Services.Implementations
{
    public class PostService : IPostService
    {
        private readonly IPostRepository _repo;
        private readonly ProjectSettings _settings;

        public PostService(IPostRepository repo, IOptions<ProjectSettings> options)
        {
            _repo = repo;
            _settings = options.Value;
        }

        public async Task<Result<List<Post>>> GetAllPostsAsync()
        {
            var posts = (await _repo.GetAllAsync()).Where(p => !p.IsDeleted).ToList();
            Log.Information("Отримано список усіх постів. Кількість: {Count}", posts.Count);

            return posts;
        }

        public async Task<Result<Post>> GetPostByIdAsync(int id)
        {
            var post = await _repo.GetByIdAsync(id);
            if (post == null)
            {
                Log.Warning("Товар з ID {PostId} не знайдено", id);
                return $"Товар з ID {id} не знайдено.";
            }

            return post;
        }

        public async Task<Result<Post>> CreatePostAsync(CreatePostDto dto, string sellerId)
        {
            Log.Information("Початок створення поста '{Title}' для продавця {SellerId}", dto.Title, sellerId);

            var post = new Post
            {
                Title = dto.Title,
                Description = dto.Description,
                Price = dto.Price,
                CategoryId = dto.CategoryId,
                SellerId = sellerId,
                IsDeleted = false,
            };

            await _repo.AddAsync(post);

            Log.Information("Пост успішно створено. ID поста: {PostId}", post.Id);

            return post;
        }

        public async Task<Result<Post>> UpdatePostAsync(int id, UpdatePostDto dto)
        {
            Log.Information("Запит на оновлення поста {PostId}", id);

            var post = await _repo.GetByIdAsync(id);
            if (post == null || post.IsDeleted)
            {
                Log.Warning("Невдала спроба оновлення: пост {PostId} не знайдено", id);
                return "Товар для оновлення не знайдено.";
            }

            post.Title = dto.Title ?? post.Title;
            post.Description = dto.Description ?? post.Description;
            post.Price = dto.Price ?? post.Price;
            post.CategoryId = dto.CategoryId ?? post.CategoryId;

            await _repo.UpdateAsync(post);

            Log.Information("Пост {PostId} успішно оновлено", id);

            return post;
        }

        public async Task<Result> DeletePostAsync(int id, string deleterId)
        {
            Log.Information("Запит на видалення поста {PostId}", id);

            var post = await _repo.GetByIdAsync(id);
            if (post == null)
            {
                Log.Warning("Невдала спроба видалення: пост {PostId} не знайдено", id);
                return "Товар для видалення не знайдено.";
            }

            post.IsDeleted = true;
            post.DeletedByUserId = deleterId;
            await _repo.UpdateAsync(post);

            Log.Information("Пост {PostId} успішно переміщено в кошик видалених товарів", id);

            return Result.Success();
        }

        public async Task<Result<List<Post>>> GetFilteredPostsAsync(string? searchTerm, int? categoryId, string? sortBy, int page = 1, string searchType = "products")
        {
            var postsQuery = (await _repo.GetAllAsync()).AsQueryable().Where(p => !p.IsDeleted);

            if (categoryId.HasValue && categoryId > 0)
            {
                postsQuery = postsQuery.Where(p => p.CategoryId == categoryId);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm) && searchTerm.Length >= _settings.Pagination.MinSearchLength)
            {
                var lowerSearch = searchTerm.ToLower().Trim();

                if (searchType == "sellers")
                {
                    postsQuery = postsQuery.Where(p => p.Seller != null && p.Seller.UserName != null && p.Seller.UserName.ToLower().Contains(lowerSearch));
                }
                else
                {
                    postsQuery = postsQuery.Where(p => (p.Title != null && p.Title.ToLower().Contains(lowerSearch)) ||
                                                       (p.Description != null && p.Description.ToLower().Contains(lowerSearch)));
                }
            }

            postsQuery = sortBy switch
            {
                "price_asc" => postsQuery.OrderBy(p => p.Price),
                "price_desc" => postsQuery.OrderByDescending(p => p.Price),
                "rating" => postsQuery.OrderByDescending(p => p.Rating),
                "newest" => postsQuery.OrderByDescending(p => p.CreatedAt),
                _ => postsQuery.OrderByDescending(p => p.CreatedAt)
            };

            int pageSize = _settings.Pagination.DefaultPageSize;
            var resultList = postsQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            Log.Information("Фільтрація завершена. Знайдено: {Count} для сторінки {Page}. Тип пошуку: {SearchType}", resultList.Count, page, searchType);

            return resultList;
        }

        public async Task<Result<List<Post>>> SearchPostsAsync(string searchTerm)
            => await GetFilteredPostsAsync(searchTerm, null, null, 1, "products");

        public async Task<Result<List<Post>>> GetDeletedPostsAsync(string? userId = null)
        {
            var query = (await _repo.GetAllAsync()).AsQueryable().Where(p => p.IsDeleted);

            if (!string.IsNullOrEmpty(userId))
            {
                query = query.Where(p => p.DeletedByUserId == userId);
            }

            var deletedList = query.OrderByDescending(p => p.CreatedAt).ToList();
            Log.Information("Отримано список видалених постів. Знайдено: {Count}", deletedList.Count);

            return deletedList;
        }

        public async Task<Result> RestorePostAsync(int id)
        {
            Log.Information("Запит на відновлення поста {PostId} з кошика видалених товарів", id);

            var post = await _repo.GetByIdAsync(id);
            if (post == null)
            {
                return "Товар для відновлення не знайдено.";
            }

            post.IsDeleted = false;
            post.DeletedByUserId = null;

            await _repo.UpdateAsync(post);

            Log.Information("Пост {PostId} успішно відновлено і повернуто в каталог", id);

            return Result.Success();
        }

        public async Task<Result> HardDeletePostAsync(int id)
        {
            Log.Information("Запит на ПОВНЕ видалення поста {PostId} з бази даних", id);

            var post = await _repo.GetByIdAsync(id);
            if (post == null)
            {
                return "Товар не знайдено.";
            }

            await _repo.DeleteAsync(post);

            Log.Information("Пост {PostId} остаточно видалено з бази даних", id);

            return Result.Success();
        }

        public async Task<Result<List<Post>>> GetTopRatedPostsAsync(int count = 4)
        {
            Log.Information("Запит на отримання {Count} товарів з найвищим рейтингом", count);

            var query = (await _repo.GetAllAsync())
                .AsQueryable()
                .Where(p => !p.IsDeleted);

            var topPosts = query
                .OrderByDescending(p => p.Rating)
                .Take(count)
                .ToList();

            return topPosts;
        }

        public async Task<Result<List<Post>>> GetPostsBySellerIdAsync(string sellerId, string? searchTerm = null)
        {
            var query = (await _repo.GetAllAsync())
                .AsQueryable()
                .Where(p => p.SellerId == sellerId && !p.IsDeleted);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var lowerTerm = searchTerm.ToLower();
                query = query.Where(p =>
                    (p.Title != null && p.Title.ToLower().Contains(lowerTerm)) ||
                    (p.Description != null && p.Description.ToLower().Contains(lowerTerm)));
            }

            var resultList = query.OrderByDescending(p => p.CreatedAt).ToList();

            Log.Information("Отримано {Count} товарів для вітрини продавця {SellerId}", resultList.Count, sellerId);

            return resultList;
        }
    }
}