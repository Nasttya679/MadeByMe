using MadeByMe.Domain.Entities;
using MadeByMe.Application.Services.Interfaces;
using MadeByMe.Infrastructure.Repositories.Interfaces;
using MadeByMe.Application.DTOs;

namespace MadeByMe.Application.Services.Implementations
{
        
    public class PostService : IPostService
    {
        private readonly IPostRepository _repo;
        public PostService(IPostRepository repo) => _repo = repo;

        public List<Post> GetAllPosts() => _repo.GetAll();
        public Post? GetPostById(int id) => _repo.GetById(id);

        public Post CreatePost(CreatePostDto dto, string sellerId)
        {
            var post = new Post
            {
                Title = dto.Title,
                Description = dto.Description,
                Price = dto.Price,
                CategoryId = dto.CategoryId,
                SellerId = sellerId
            };
            _repo.Add(post);
            return post;
        }

        public Post? UpdatePost(int id, UpdatePostDto dto)
        {
            var post = _repo.GetById(id);
            if (post == null) return null;

            post.Title = dto.Title ?? post.Title;
            post.Description = dto.Description ?? post.Description;
            post.Price = dto.Price ?? post.Price;
            post.CategoryId = dto.CategoryId ?? post.CategoryId;

            _repo.Update(post);
            return post;
        }

        public bool DeletePost(int id)
        {
            var post = _repo.GetById(id);
            if (post == null) return false;

            _repo.Delete(post);
            return true;
        }

        public List<Post> SearchPosts(string searchTerm)
        {
            var posts = _repo.GetAll();
            if (!string.IsNullOrEmpty(searchTerm))
            {
                posts = posts.Where(p => p.Title.Contains(searchTerm) || p.Description.Contains(searchTerm)).ToList();
            }
            return posts;
        }
    }
}