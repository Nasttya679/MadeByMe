
using MadeByMe.Domain.Entities;
using MadeByMe.Application.DTOs;


namespace MadeByMe.Application.Services.Interfaces
{
    public interface IPostService
    {
        List<Post> GetAllPosts();
        Post? GetPostById(int id);
        Post CreatePost(CreatePostDto createPostDto, string sellerId);
        Post? UpdatePost(int id, UpdatePostDto updatePostDto);
        bool DeletePost(int id);
        List<Post> SearchPosts(string searchTerm);
    }
}
