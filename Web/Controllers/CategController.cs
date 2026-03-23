using MadeByMe.Application.Services;
using MadeByMe.Application.DTOs;
using Microsoft.AspNetCore.Mvc;
using MadeByMe.Infrastructure.Data;
using MadeByMe.Application.ViewModels;
using MadeByMe.Application.Services.Interfaces;

namespace MadeByMe.src.Controllers
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
			var categories = _categoryService.GetAllCategories();
			return View(categories);
		}

		public IActionResult Details(int id)
		{
			var category = _categoryService.GetCategoryById(id);
			if (category == null) return NotFound();

			return View(category);
		}

		public IActionResult Create()
		{
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Create(CreateCategoryDto createCategoryDto)
		{
			if (!ModelState.IsValid) return View(createCategoryDto);

			_categoryService.CreateCategory(createCategoryDto);
			return RedirectToAction(nameof(Index));
		}

        public IActionResult Edit(int id)
        {
            var category = _categoryService.GetCategoryById(id);
            if (category == null) return NotFound();

            var updateDto = new UpdateCategoryDto
            {
                Name = category.Name
            };
            return View(updateDto);
        }

        [HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Edit(int id, UpdateCategoryDto updateCategoryDto)
		{
			if (!ModelState.IsValid) return View(updateCategoryDto);

			var updatedCategory = _categoryService.UpdateCategory(id, updateCategoryDto);
			if (updatedCategory == null) return NotFound();

			return RedirectToAction(nameof(Index));
		}

		public IActionResult Delete(int id)
		{
			var category = _categoryService.GetCategoryById(id);
			if (category == null) return NotFound();

			return View(category);
		}

		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public IActionResult DeleteConfirmed(int id)
		{
			if (!_categoryService.DeleteCategory(id)) return NotFound();

			return RedirectToAction(nameof(Index));
		}

	}
}
