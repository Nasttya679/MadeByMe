using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace MadeByMe.Web.Controllers
{
    public abstract class BaseController : Controller
    {
        protected string? CurrentUserId => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        protected string? CurrentUserName => User.Identity?.Name;

        protected bool IsAuthenticated => User.Identity?.IsAuthenticated ?? false;

        protected void SetErrorMessage(string message)
        {
            TempData["Error"] = message;
        }

        protected void SetSuccessMessage(string message)
        {
            TempData["Success"] = message;
        }

        protected void AddErrorToModelState(string message, string key = "")
        {
            ModelState.AddModelError(key, message);
        }
    }
}