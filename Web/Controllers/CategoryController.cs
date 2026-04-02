using MadeByMe.Application.DTOs;
using MadeByMe.Application.Services.Interfaces;
using MadeByMe.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace MadeByMe.Web.Controllers
{
    public class CategoryController : BaseController
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index()
        {
            var result = await _categoryService.GetAllCategoriesAsync();

            if (result.IsFailure)
            {
                Log.Warning("Не вдалося отримати список категорій. Причина: {ErrorMessage}", result.ErrorMessage);
                SetErrorMessage(result.ErrorMessage);
                return View(new List<Category>());
            }

            return View(result.Value);
        }

        public async Task<IActionResult> Details(int id)
        {
            var result = await _categoryService.GetCategoryByIdAsync(id);

            if (result.IsFailure)
            {
                Log.Warning("Не вдалося знайти детальну інформацію про категорію {CategoryId}. Причина: {ErrorMessage}", id, result.ErrorMessage);
                return NotFound(result.ErrorMessage);
            }

            return View(result.Value);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(CreateCategoryDto createCategoryDto)
        {
            if (!ModelState.IsValid)
            {
                Log.Warning("Помилка валідації форми при спробі створити нову категорію");
                return View(createCategoryDto);
            }

            var result = await _categoryService.CreateCategoryAsync(createCategoryDto);

            if (result.IsFailure)
            {
                Log.Warning("Не вдалося створити категорію. Причина: {ErrorMessage}", result.ErrorMessage);
                AddErrorToModelState(result.ErrorMessage);
                return View(createCategoryDto);
            }

            Log.Information("Нову категорію успішно створено");
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var result = await _categoryService.GetCategoryByIdAsync(id);

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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, UpdateCategoryDto updateCategoryDto)
        {
            if (!ModelState.IsValid)
            {
                Log.Warning("Помилка валідації форми при редагуванні категорії {CategoryId}", id);
                return View(updateCategoryDto);
            }

            var result = await _categoryService.UpdateCategoryAsync(id, updateCategoryDto);

            if (result.IsFailure)
            {
                Log.Warning("Не вдалося оновити категорію {CategoryId}. Причина: {ErrorMessage}", id, result.ErrorMessage);
                AddErrorToModelState(result.ErrorMessage);
                return View(updateCategoryDto);
            }

            Log.Information("Категорію {CategoryId} успішно оновлено", id);
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _categoryService.GetCategoryByIdAsync(id);

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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _categoryService.DeleteCategoryAsync(id);

            if (result.IsFailure)
            {
                Log.Warning("Не вдалося видалити категорію {CategoryId}. Причина: {ErrorMessage}", id, result.ErrorMessage);
                SetErrorMessage(result.ErrorMessage);
                return RedirectToAction(nameof(Index));
            }

            Log.Information("Категорію {CategoryId} успішно видалено", id);
            return RedirectToAction(nameof(Index));
        }
    }
}