using MadeByMe.Application.DTOs;
using MadeByMe.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MadeByMe.Web.Controllers
{
    [Authorize]
    public class ComplaintController : BaseController
    {
        private readonly IComplaintService _complaintService;

        public ComplaintController(IComplaintService complaintService)
        {
            _complaintService = complaintService;
        }

        [HttpGet]
        public IActionResult Create(int? postId, string? sellerId)
        {
            if (postId == null && string.IsNullOrEmpty(sellerId))
            {
                SetErrorMessage("Не вказано об'єкт для скарги.");
                return RedirectToAction("Index", "Home");
            }

            var dto = new ComplaintDto
            {
                PostId = postId,
                SellerId = sellerId,
            };

            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ComplaintDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            var result = await _complaintService.SubmitComplaintAsync(CurrentUserId!, dto);

            if (result.IsFailure)
            {
                SetErrorMessage(result.ErrorMessage);
                return View(dto);
            }

            SetSuccessMessage("Вашу скаргу успішно надіслано на розгляд модераторам. Дякуємо за пильність!");

            if (dto.PostId.HasValue)
            {
                return RedirectToAction("Details", "Post", new { id = dto.PostId.Value });
            }

            return RedirectToAction("Index", "Home");
        }
    }
}