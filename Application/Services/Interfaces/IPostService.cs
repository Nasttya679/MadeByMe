using System.Collections.Generic;
using MadeByMe.Application.Common;
using MadeByMe.Application.DTOs;
using MadeByMe.Domain.Entities;

namespace MadeByMe.Application.Services.Interfaces
{
    public interface IPostService
    {
        Result<List<Post>> GetAllPosts();

        Result<Post> GetPostById(int id);

        Result<Post> CreatePost(CreatePostDto createPostDto, string sellerId);

        Result<Post> UpdatePost(int id, UpdatePostDto updatePostDto);

        Result DeletePost(int id);

        Result<List<Post>> SearchPosts(string searchTerm);
    }
}