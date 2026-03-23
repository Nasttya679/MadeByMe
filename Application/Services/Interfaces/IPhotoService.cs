

using MadeByMe.Domain.Entities;
using MadeByMe.Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace MadeByMe.Application.Services.Interfaces {
    public interface IPhotoService
    {
        Task<Photo> SavePhotoAsync(IFormFile file, int? postId = null);
        void DeletePhoto(Photo photo);
        string GetPhotoUrl(Photo photo);
    }
}   