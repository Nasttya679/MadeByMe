using MadeByMe.Application.DTOs;
using MadeByMe.Application.Services.Interfaces;
using MadeByMe.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

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
                return View(createCategoryDto);
            }

            var result = _categoryService.CreateCategory(createCategoryDto);

            if (result.IsFailure)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage);
                return View(createCategoryDto);
            }

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            var result = _categoryService.GetCategoryById(id);

            if (result.IsFailure)
            {
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
                return View(updateCategoryDto);
            }

            var result = _categoryService.UpdateCategory(id, updateCategoryDto);

            if (result.IsFailure)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage);
                return View(updateCategoryDto);
            }

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Delete(int id)
        {
            var result = _categoryService.GetCategoryById(id);

            if (result.IsFailure)
            {
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
                TempData["Error"] = result.ErrorMessage;
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Index));
        }
    }
}