using MadeByMe.Application.DTOs;
using MadeByMe.Application.Services.Interfaces;
using MadeByMe.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MadeByMe.Web.Controllers
{
    public class CommentController : Controller
    {
        private readonly ICommentService _commentService;
        private readonly UserManager<ApplicationUser> _userManager;

        public CommentController(ICommentService commentService, UserManager<ApplicationUser> userManager)
        {
            _commentService = commentService;
            _userManager = userManager;
        }

        /*
        public IActionResult Index()
        {
           var result = _commentService.GetAllComments();
           if (result.IsFailure) return View(new List<Comment>());
           return View(result.Value);
        }
        */

        public IActionResult Details(int id)
        {
            var result = _commentService.GetCommentById(id);

            if (result.IsFailure)
            {
                return NotFound(result.ErrorMessage);
            }

            return View(result.Value);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CreateCommentDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            var userId = _userManager.GetUserId(User);

            var result = _commentService.AddComment(dto, userId!);

            if (result.IsFailure)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage);
                return View(dto);
            }

            return RedirectToAction("Details", "Post", new { id = result.Value.PostId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Delete(int id)
        {
            var result = _commentService.GetCommentById(id);
            if (result.IsFailure)
            {
                return NotFound(result.ErrorMessage);
            }

            var comment = result.Value;
            var currentUserName = User.Identity?.Name;

            if (User.IsInRole("Admin") || (comment.User != null && comment.User.UserName == currentUserName))
            {
                var deleteResult = _commentService.DeleteComment(id);

                if (deleteResult.IsFailure)
                {
                    TempData["Error"] = deleteResult.ErrorMessage;
                }

                return RedirectToAction("Details", "Post", new { id = comment.PostId });
            }

            return Forbid();
        }
    }
}