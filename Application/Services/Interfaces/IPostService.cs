using MadeByMe.Application.Common;
using MadeByMe.Application.DTOs;
using MadeByMe.Domain.Entities;

namespace MadeByMe.Application.Services.Interfaces
{
    public interface IPostService
    {
        Task<Result<List<Post>>> GetAllPostsAsync();

        Task<Result<Post>> GetPostByIdAsync(int id);

        Task<Result<Post>> CreatePostAsync(CreatePostDto createPostDto, string sellerId);

        Task<Result<Post>> UpdatePostAsync(int id, UpdatePostDto updatePostDto);

        Task<Result<List<Post>>> GetFilteredPostsAsync(string? searchTerm, int? categoryId, string? sortBy, int page = 1, string searchType = "products");

        Task<Result> DeletePostAsync(int id, string deleterId);

        Task<Result<List<Post>>> GetDeletedPostsAsync(string? userId = null);

        Task<Result> RestorePostAsync(int id);

        Task<Result> HardDeletePostAsync(int id);

        Task<Result<List<Post>>> GetTopRatedPostsAsync(int count = 4);

        Task<Result<List<Post>>> GetPostsBySellerIdAsync(string sellerId, string? searchTerm = null);
    }
}