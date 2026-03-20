using MadeByMe.DTOs;
using MadeByMe.Models;
using MadeByMe.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace MadeByMe.Services
{
    public class PostService
    {
        private readonly ApplicationDbContext _context;

        public PostService(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Post> GetAllPosts()
        {
            return _context.Posts
                .Include(p => p.Category)
                .Include(p => p.Seller)
                .Include(p => p.Photos)
                .ToList();
        }

        // Змінено тип на Post?, бо товар може бути не знайдений
        public Post? GetPostById(int id)
        {
            return _context.Posts
                .Include(p => p.Category)
                .Include(p => p.Seller)
                .Include(p => p.CommentsList)
                .Include(p => p.Photos)
                .FirstOrDefault(p => p.Id == id);
        }

        public Post CreatePost(CreatePostDto createPostDtoDto, string sellerId)
        {
            var post = new Post
            {
                // Додаємо ?? string.Empty, щоб гарантувати, що значення ніколи не буде null
                Title = createPostDtoDto.Title ?? string.Empty,
                Description = createPostDtoDto.Description ?? string.Empty,
                Price = createPostDtoDto.Price,
                CategoryId = createPostDtoDto.CategoryId,
                SellerId = sellerId
            };

            _context.Posts.Add(post);
            _context.SaveChanges();
            return post;
        }

        // Змінено тип на Post?
        public Post? UpdatePost(int id, UpdatePostDto updatePostDtoDto)
        {
            var post = _context.Posts.Find(id);
            if (post != null)
            {
                post.Title = updatePostDtoDto.Title ?? post.Title;
                post.Description = updatePostDtoDto.Description ?? post.Description;
                post.Price = updatePostDtoDto.Price ?? post.Price;
                post.CategoryId = updatePostDtoDto.CategoryId ?? post.CategoryId;

                _context.SaveChanges();
            }

            // Додано порожній рядок після }
            return post;
        }

        public bool DeletePost(int id)
        {
            var post = _context.Posts.Find(id);
            if (post != null)
            {
                _context.Posts.Remove(post);
                _context.SaveChanges();
                return true;
            }

            // Додано порожній рядок після }
            return false;
        }

        public List<Post> SearchPosts(string searchTerm)
        {
            var query = _context.Posts
                .Include(p => p.Category)
                .Include(p => p.Seller)
                .Include(p => p.Photos)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(p => p.Title.Contains(searchTerm) || p.Description.Contains(searchTerm));
            }

            return query.ToList();
        }
    }
}