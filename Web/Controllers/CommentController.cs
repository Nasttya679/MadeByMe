using MadeByMe.Application.DTOs;
using MadeByMe.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace MadeByMe.Web.Controllers
{
    public class CommentController : BaseController
    {
        private readonly ICommentService _commentService;

        public CommentController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        public async Task<IActionResult> Details(int id)
        {
            var result = await _commentService.GetCommentByIdAsync(id);

            if (result.IsFailure)
            {
                Log.Warning("Коментар з ID {CommentId} не знайдено. Причина: {ErrorMessage}", id, result.ErrorMessage);
                return NotFound(result.ErrorMessage);
            }

            return View(result.Value);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create(CreateCommentDto dto)
        {
            if (!ModelState.IsValid)
            {
                Log.Warning("Помилка валідації при спробі створення коментаря до поста {PostId}", dto.PostId);
                return View(dto);
            }

            var result = await _commentService.AddCommentAsync(dto, CurrentUserId!);

            if (result.IsFailure)
            {
                Log.Warning("Не вдалося додати коментар до поста {PostId}. Причина: {ErrorMessage}", dto.PostId, result.ErrorMessage);
                AddErrorToModelState(result.ErrorMessage);
                return View(dto);
            }

            Log.Information("Користувач {UserId} успішно додав коментар до поста {PostId}", CurrentUserId, dto.PostId);
            return RedirectToAction("Details", "Post", new { id = result.Value.PostId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _commentService.GetCommentByIdAsync(id);

            if (result.IsFailure)
            {
                return NotFound(result.ErrorMessage);
            }

            var comment = result.Value;
            var currentUserId = CurrentUserId;

            if (User.IsInRole("Admin") || comment.UserId == currentUserId)
            {
                var deleteResult = await _commentService.DeleteCommentAsync(id);

                if (deleteResult.IsFailure)
                {
                    return NotFound(deleteResult.ErrorMessage);
                }

                return RedirectToAction("Details", "Post", new { id = comment.PostId });
            }

            return Forbid();
        }
    }
}