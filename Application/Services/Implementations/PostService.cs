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

        public Result<List<Post>> GetAllPosts()
        {
            var posts = _repo.GetAll();
            Log.Information("Отримано список усіх постів. Кількість: {Count}", posts.Count);
            return Result<List<Post>>.Success(posts);
        }

        public Result<Post> GetPostById(int id)
        {
            var post = _repo.GetById(id);
            if (post == null)
            {
                Log.Warning("Товар з ID {PostId} не знайдено", id);
                return Result<Post>.Failure($"Товар з ID {id} не знайдено.");
            }

            return Result<Post>.Success(post);
        }

        public Result<Post> CreatePost(CreatePostDto dto, string sellerId)
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

            _repo.Add(post);

            Log.Information("Пост успішно створено. ID поста: {PostId}", post.Id);
            return Result<Post>.Success(post);
        }

        public Result<Post> UpdatePost(int id, UpdatePostDto dto)
        {
            Log.Information("Запит на оновлення поста {PostId}", id);

            var post = _repo.GetById(id);
            if (post == null)
            {
                Log.Warning("Невдала спроба оновлення: пост {PostId} не знайдено", id);
                return Result<Post>.Failure("Товар для оновлення не знайдено.");
            }

            post.Title = dto.Title ?? post.Title;
            post.Description = dto.Description ?? post.Description;
            post.Price = dto.Price ?? post.Price;
            post.CategoryId = dto.CategoryId ?? post.CategoryId;

            _repo.Update(post);

            Log.Information("Пост {PostId} успішно оновлено", id);
            return Result<Post>.Success(post);
        }

        public Result DeletePost(int id)
        {
            Log.Information("Запит на видалення поста {PostId}", id);

            var post = _repo.GetById(id);
            if (post == null)
            {
                Log.Warning("Невдала спроба видалення: пост {PostId} не знайдено", id);
                return Result.Failure("Товар для видалення не знайдено.");
            }

            _repo.Delete(post);

            Log.Information("Пост {PostId} успішно видалено з бази даних", id);
            return Result.Success();
        }

        public Result<List<Post>> GetFilteredPosts(string? searchTerm, int? categoryId, string? sortBy)
        {
            Log.Information("Фільтрація постів: Пошук='{SearchTerm}', Категорія={CategoryId}, Сортування={SortBy}", searchTerm, categoryId, sortBy);

            var posts = _repo.GetAll().AsQueryable();

            if (categoryId.HasValue && categoryId > 0)
            {
                posts = posts.Where(p => p.CategoryId == categoryId);
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                var lowerSearch = searchTerm.ToLower();
                posts = posts.Where(p => p.Title!.ToLower().Contains(lowerSearch) ||
                                         p.Description!.ToLower().Contains(lowerSearch));
            }

            posts = sortBy switch
            {
                "price_asc" => posts.OrderBy(p => p.Price),
                "price_desc" => posts.OrderByDescending(p => p.Price),
                "rating" => posts.OrderByDescending(p => p.Rating),
                "newest" => posts.OrderByDescending(p => p.CreatedAt),
                _ => posts.OrderByDescending(p => p.CreatedAt)
            };

            var resultList = posts.ToList();
            Log.Information("Фільтрація завершена. Знайдено: {Count}", resultList.Count);

            return Result<List<Post>>.Success(resultList);
        }
    }
}