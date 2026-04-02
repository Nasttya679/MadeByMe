using MadeByMe.Application.DTOs;
using MadeByMe.Application.Services.Interfaces;
using MadeByMe.Domain.Entities;
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

        /*
        public async Task<IActionResult> Index()
        {
           var result = await _commentService.GetAllCommentsAsync();
           if (result.IsFailure) return View(new List<Comment>());
           return View(result.Value);
        }
        */

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

            var userId = CurrentUserId;

            var result = await _commentService.AddCommentAsync(dto, userId!);

            if (result.IsFailure)
            {
                Log.Warning("Не вдалося додати коментар до поста {PostId} для користувача {UserId}. Причина: {ErrorMessage}", dto.PostId, userId, result.ErrorMessage);
                AddErrorToModelState(result.ErrorMessage);
                return View(dto);
            }

            Log.Information("Користувач {UserId} успішно додав коментар до поста {PostId}", userId, dto.PostId);
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
                Log.Warning("Спроба видалення: коментар з ID {CommentId} не знайдено", id);
                return NotFound(result.ErrorMessage);
            }

            var comment = result.Value;
            var currentUserName = CurrentUserName;

            if (User.IsInRole("Admin") || (comment.User != null && comment.User.UserName == currentUserName))
            {
                var deleteResult = await _commentService.DeleteCommentAsync(id);

                if (deleteResult.IsFailure)
                {
                    Log.Error("Не вдалося видалити коментар {CommentId}. Причина: {ErrorMessage}", id, deleteResult.ErrorMessage);
                    SetErrorMessage(deleteResult.ErrorMessage);
                }
                else
                {
                    Log.Information("Коментар {CommentId} успішно видалено користувачем {UserName}", id, currentUserName);
                }

                return RedirectToAction("Details", "Post", new { id = comment.PostId });
            }

            Log.Warning("Користувач {UserName} намагався видалити коментар {CommentId} без відповідних прав доступу", currentUserName, id);
            return Forbid();
        }
    }
}