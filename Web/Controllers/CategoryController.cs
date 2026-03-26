using MadeByMe.Application.DTOs;
using MadeByMe.Application.Services.Interfaces;
using MadeByMe.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace MadeByMe.Web.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public IActionResult Index()
        {
            var result = _categoryService.GetAllCategories();

            if (result.IsFailure)
            {
                Log.Warning("Не вдалося отримати список категорій. Причина: {ErrorMessage}", result.ErrorMessage);
                TempData["Error"] = result.ErrorMessage;
                return View(new List<Category>());
            }

            return View(result.Value);
        }

        public IActionResult Details(int id)
        {
            var result = _categoryService.GetCategoryById(id);

            if (result.IsFailure)
            {
                Log.Warning("Не вдалося знайти детальну інформацію про категорію {CategoryId}. Причина: {ErrorMessage}", id, result.ErrorMessage);
                return NotFound(result.ErrorMessage);
            }

            return View(result.Value);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CreateCategoryDto createCategoryDto)
        {
            if (!ModelState.IsValid)
            {
                Log.Warning("Помилка валідації форми при спробі створити нову категорію");
                return View(createCategoryDto);
            }

            var result = _categoryService.CreateCategory(createCategoryDto);

            if (result.IsFailure)
            {
                Log.Warning("Не вдалося створити категорію. Причина: {ErrorMessage}", result.ErrorMessage);
                ModelState.AddModelError(string.Empty, result.ErrorMessage);
                return View(createCategoryDto);
            }

            Log.Information("Нову категорію успішно створено");
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            var result = _categoryService.GetCategoryById(id);

            if (result.IsFailure)
            {
                Log.Warning("Не вдалося завантажити категорію {CategoryId} для редагування. Причина: {ErrorMessage}", id, result.ErrorMessage);
                return NotFound(result.ErrorMessage);
            }

            var updateDto = new UpdateCategoryDto
            {
                Name = result.Value.Name,
            };
            return View(updateDto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, UpdateCategoryDto updateCategoryDto)
        {
            if (!ModelState.IsValid)
            {
                Log.Warning("Помилка валідації форми при редагуванні категорії {CategoryId}", id);
                return View(updateCategoryDto);
            }

            var result = _categoryService.UpdateCategory(id, updateCategoryDto);

            if (result.IsFailure)
            {
                Log.Warning("Не вдалося оновити категорію {CategoryId}. Причина: {ErrorMessage}", id, result.ErrorMessage);
                ModelState.AddModelError(string.Empty, result.ErrorMessage);
                return View(updateCategoryDto);
            }

            Log.Information("Категорію {CategoryId} успішно оновлено", id);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Delete(int id)
        {
            var result = _categoryService.GetCategoryById(id);

            if (result.IsFailure)
            {
                Log.Warning("Не вдалося завантажити сторінку видалення для категорії {CategoryId}. Причина: {ErrorMessage}", id, result.ErrorMessage);
                return NotFound(result.ErrorMessage);
            }

            return View(result.Value);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var result = _categoryService.DeleteCategory(id);

            if (result.IsFailure)
            {
                Log.Warning("Не вдалося видалити категорію {CategoryId}. Причина: {ErrorMessage}", id, result.ErrorMessage);
                TempData["Error"] = result.ErrorMessage;
                return RedirectToAction(nameof(Index));
            }

            Log.Information("Категорію {CategoryId} успішно видалено", id);
            return RedirectToAction(nameof(Index));
        }
    }
}