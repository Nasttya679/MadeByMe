using MadeByMe.Application.Common;
using MadeByMe.Application.DTOs;
using MadeByMe.Application.Services.Interfaces;
using MadeByMe.Domain.Entities;
using MadeByMe.Infrastructure.Repositories.Interfaces;
using Serilog;

namespace MadeByMe.Application.Services.Implementations
{
    public class PostService : IPostService
    {
        private readonly IPostRepository _repo;

        public PostService(IPostRepository repo) => _repo = repo;

        public async Task<Result<List<Post>>> GetAllPostsAsync()
        {
            var posts = await _repo.GetAllAsync();
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
            };

            await _repo.AddAsync(post);

            Log.Information("Пост успішно створено. ID поста: {PostId}", post.Id);

            return post;
        }

        public async Task<Result<Post>> UpdatePostAsync(int id, UpdatePostDto dto)
        {
            Log.Information("Запит на оновлення поста {PostId}", id);

            var post = await _repo.GetByIdAsync(id);
            if (post == null)
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

        public async Task<Result> DeletePostAsync(int id)
        {
            Log.Information("Запит на видалення поста {PostId}", id);

            var post = await _repo.GetByIdAsync(id);
            if (post == null)
            {
                Log.Warning("Невдала спроба видалення: пост {PostId} не знайдено", id);
                return "Товар для видалення не знайдено.";
            }

            await _repo.DeleteAsync(post);

            Log.Information("Пост {PostId} успішно видалено з бази даних", id);

            return Result.Success();
        }

        public async Task<Result<List<Post>>> SearchPostsAsync(string searchTerm)
        {
            Log.Information("Фільтрація постів: Пошук='{SearchTerm}', Категорія={CategoryId}, Сортування={SortBy}", searchTerm, categoryId, sortBy);

            var posts = _repo.GetAll().AsQueryable();

            if (categoryId.HasValue && categoryId > 0)
            {
                posts = posts.Where(p => p.CategoryId == categoryId);
            }

            var posts = await _repo.GetAllAsync();
            if (!string.IsNullOrEmpty(searchTerm))
            {
                var lowerSearch = searchTerm.ToLower();
                posts = posts.Where(p => p.Title!.ToLower().Contains(lowerSearch) ||
                                         p.Description!.ToLower().Contains(lowerSearch));
            }

            Log.Information("Пошук завершено. Знайдено результатів: {Count}", posts.Count);
            posts = sortBy switch
            {
                "price_asc" => posts.OrderBy(p => p.Price),
                "price_desc" => posts.OrderByDescending(p => p.Price),
                "rating" => posts.OrderByDescending(p => p.Rating),
                "newest" => posts.OrderByDescending(p => p.CreatedAt),
                _ => posts.OrderByDescending(p => p.CreatedAt)
            };

            return posts;
        }
    }
}