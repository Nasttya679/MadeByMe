using MadeByMe.Application.Common;
using MadeByMe.Application.Services.Interfaces;
using MadeByMe.Domain.Entities;
using MadeByMe.Infrastructure.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Serilog;

namespace MadeByMe.Application.Services.Implementations
{
    public class PhotoService : IPhotoService
    {
        private readonly string _uploadPath;
        private readonly IPhotoRepository _photoRepository;

        public PhotoService(IPhotoRepository photoRepository)
        {
            _photoRepository = photoRepository;
            _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
            EnsureDirectoryExists();
        }

        public async Task<Result<Photo>> SavePhotoAsync(IFormFile file, int? postId = null)
        {
            if (file == null || file.Length == 0)
            {
                Log.Warning("Спроба завантаження порожнього файлу або файл відсутній (PostId: {PostId})", postId);

                return "Файл не завантажено або він порожній.";
            }

            EnsureDirectoryExists();

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(_uploadPath, fileName);

            Log.Information("Розпочато збереження файлу {FileName} на диск для поста {PostId}", fileName, postId);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var photo = new Photo
            {
                FileName = fileName,
                FilePath = $"/images/{fileName}",
                ContentType = file.ContentType,
                FileSize = file.Length,
                PostId = postId,
            };

            await _photoRepository.AddAsync(photo);

            Log.Information("Файл {FileName} успішно збережено на диск та додано в базу (Size: {FileSize} bytes)", fileName, file.Length);

            return photo;
        }

        public async Task<Result> DeletePhotoAsync(Photo photo)
        {
            if (photo == null || string.IsNullOrEmpty(photo.FileName))
            {
                Log.Warning("Запит на видалення фото відхилено: об'єкт фото порожній");

                return "Фото не передано для видалення або ім'я файлу порожнє.";
            }

            var filePath = Path.Combine(_uploadPath, photo.FileName);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                Log.Information("Файл {FileName} успішно видалено з диска", photo.FileName);
            }
            else
            {
                Log.Warning("Файл {FileName} не знайдено на диску, видаляється лише запис із бази", photo.FileName);
            }

            await _photoRepository.DeleteAsync(photo);
            Log.Information("Запис про фото {FileName} успішно видалено з бази даних", photo.FileName);

            return Result.Success();
        }

        public string GetPhotoUrl(Photo photo)
        {
            return photo?.FilePath ?? "/images/default.jpg";
        }

        private void EnsureDirectoryExists()
        {
            if (!Directory.Exists(_uploadPath))
            {
                Directory.CreateDirectory(_uploadPath);
                Log.Information("Створено папку для зображень: {Path}", _uploadPath);
            }
        }
    }
}