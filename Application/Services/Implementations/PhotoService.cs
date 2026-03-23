using MadeByMe.Domain.Entities;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;
using MadeByMe.Application.Services.Interfaces;

namespace MadeByMe.Application.Services.Implementations
{
    public class PhotoService : IPhotoService
    {
        private readonly string _uploadPath;

        public PhotoService()
        {
            _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
            EnsureDirectoryExists();
        }

        private void EnsureDirectoryExists()
        {
            if (!Directory.Exists(_uploadPath))
            {
                Directory.CreateDirectory(_uploadPath);
            }
        }

        public async Task<Photo> SavePhotoAsync(IFormFile file, int? postId = null)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("No file uploaded");

            EnsureDirectoryExists();

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(_uploadPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return new Photo
            {
                FileName = fileName,
                FilePath = $"/images/{fileName}",
                ContentType = file.ContentType,
                FileSize = file.Length,
                PostId = postId
            };
        }

        public void DeletePhoto(Photo photo)
        {
            if (photo == null)
                return;

            var filePath = Path.Combine(_uploadPath, photo.FileName);
            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error deleting photo file: {ex.Message}");
                }
            }
        }

        public string GetPhotoUrl(Photo photo)
        {
            return photo?.FilePath ?? "/images/default.jpg";
        }
    }
}