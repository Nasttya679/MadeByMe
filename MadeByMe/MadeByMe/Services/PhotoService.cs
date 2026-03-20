using MadeByMe.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MadeByMe.Services
{
    public class PhotoService
    {
        private readonly string _uploadPath;

        public PhotoService()
        {
            _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
            EnsureDirectoryExists();
        }

        public async Task<Photo> SavePhotoAsync(IFormFile file, int? postId = null)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("No file uploaded");
            }

            EnsureDirectoryExists();

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName ?? string.Empty)}";
            var filePath = Path.Combine(_uploadPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return new Photo
            {
                FileName = fileName,
                FilePath = $"/images/{fileName}",
                ContentType = file.ContentType ?? "application/octet-stream",
                FileSize = file.Length,
                PostId = postId
            };
        }

        public void DeletePhoto(Photo photo)
        {
            // Додали перевірку на порожнє ім'я файлу, щоб уникнути NullReference в Path.Combine
            if (photo == null || string.IsNullOrEmpty(photo.FileName))
            {
                return;
            }

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

        // Перемістили private метод у самий низ класу
        private void EnsureDirectoryExists()
        {
            if (!Directory.Exists(_uploadPath))
            {
                Directory.CreateDirectory(_uploadPath);
            }
        }
    }
}