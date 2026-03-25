using System.Collections.Generic;
using System.Linq;
using MadeByMe.Application.Common;
using MadeByMe.Application.DTOs;
using MadeByMe.Application.Services.Interfaces;
using MadeByMe.Domain.Entities;
using MadeByMe.Infrastructure.Repositories.Interfaces;

namespace MadeByMe.Application.Services.Implementations
{
    public class PostService : IPostService
    {
        private readonly IPostRepository _repo;

        public PostService(IPostRepository repo) => _repo = repo;

        public Result<List<Post>> GetAllPosts()
        {
            return Result<List<Post>>.Success(_repo.GetAll());
        }

        public Result<Post> GetPostById(int id)
        {
            var post = _repo.GetById(id);
            if (post == null)
            {
                return Result<Post>.Failure($"Товар з ID {id} не знайдено.");
            }

            return Result<Post>.Success(post);
        }

        public Result<Post> CreatePost(CreatePostDto dto, string sellerId)
        {
            var post = new Post
            {
                Title = dto.Title,
                Description = dto.Description,
                Price = dto.Price,
                CategoryId = dto.CategoryId,
                SellerId = sellerId,
            };
            _repo.Add(post);
            return Result<Post>.Success(post);
        }

        public Result<Post> UpdatePost(int id, UpdatePostDto dto)
        {
            var post = _repo.GetById(id);
            if (post == null)
            {
                return Result<Post>.Failure("Товар для оновлення не знайдено.");
            }

            post.Title = dto.Title ?? post.Title;
            post.Description = dto.Description ?? post.Description;
            post.Price = dto.Price ?? post.Price;
            post.CategoryId = dto.CategoryId ?? post.CategoryId;

            _repo.Update(post);
            return Result<Post>.Success(post);
        }

        public Result DeletePost(int id)
        {
            var post = _repo.GetById(id);
            if (post == null)
            {
                return Result.Failure("Товар для видалення не знайдено.");
            }

            _repo.Delete(post);
            return Result.Success();
        }

        public Result<List<Post>> SearchPosts(string searchTerm)
        {
            var posts = _repo.GetAll();
            if (!string.IsNullOrEmpty(searchTerm))
            {
                posts = posts.Where(p => p.Title!.Contains(searchTerm) || p.Description!.Contains(searchTerm)).ToList();
            }

            return Result<List<Post>>.Success(posts);
        }
    }
}