using System.Threading.Tasks;
using MadeByMe.Application.Common;
using MadeByMe.Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace MadeByMe.Application.Services.Interfaces
{
    public interface IPhotoService
    {
        Task<Result<Photo>> SavePhotoAsync(IFormFile file, int? postId = null);

        Result DeletePhoto(Photo photo);

        string GetPhotoUrl(Photo photo);
    }
}